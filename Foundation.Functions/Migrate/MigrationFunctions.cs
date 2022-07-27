using Amazon.CloudFormation;
using Amazon.CloudFormation.Model;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using Amazon.S3.Transfer;
using JetBrains.Annotations;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Mono.Unix;
using System.Diagnostics;
using Amazon.Lambda.S3Events;
using Microsoft.EntityFrameworkCore;
using YadaYada.Bisque.Annotations;
using InvalidOperationException = Amazon.CloudFormation.Model.InvalidOperationException;

namespace Data.Serverless.Migrate;

public class MigrationFunctions
{
    private readonly ITransferUtility _transferUtility;
    private readonly ILogger _logger;
    private readonly SqlConnectionStringBuilder _sqlConnectionStringBuilder;

    public MigrationFunctions([NotNull] [ItemNotNull] IOptions<SqlConnectionStringBuilder> sqlConnectionStringBuildOptions, [NotNull] ILoggerProvider loggerProvider, ITransferUtility transferUtility)
    {
        if (sqlConnectionStringBuildOptions == null) throw new ArgumentNullException(nameof(sqlConnectionStringBuildOptions));
        if (loggerProvider == null) throw new ArgumentNullException(nameof(loggerProvider));
        _transferUtility = transferUtility ?? throw new ArgumentNullException(nameof(transferUtility));
        _logger = loggerProvider.CreateLogger(this.GetType().FullName);
        _sqlConnectionStringBuilder = sqlConnectionStringBuildOptions.Value;
    }

    [LambdaFunction(MemorySize = 10240, Role = "@LambdaExecutionRole", Timeout = 900, PackageType = LambdaPackageType.Zip)]
    public async Task<CloudFormationResponse> ApplyMigration(MigrationInfoRequest request, ILambdaContext lambdaContext)
    {
        using (_logger.AddMember())
        {

            try
            {
                if (request.RequestType == CloudFormationRequest.RequestTypeEnum.Update)
                {
                    return await CloudFormationResponse.CompleteCloudFormationResponse(CloudFormationResponse.StatusEnum.Success, request, lambdaContext);
                }

                FileInfo migrationsBundle = await this.GetFileFromDataArtifacts(request.MigrationsAssemblyPath, "efbundle-linux-x64");
                ArgumentNullException.ThrowIfNull(migrationsBundle, nameof(migrationsBundle));
                ArgumentNullException.ThrowIfNull(migrationsBundle.Directory, nameof(migrationsBundle.Directory));

                using (_logger.AddScope(nameof(request.RequestType), request.RequestType))
                using (_logger.AddScope(nameof(request.MigrationName), request.MigrationName))
                using (_logger.AddScope(nameof(request.InitialCatalog), request.InitialCatalog))
                using (_logger.AddScope(nameof(request.StackName), request.StackName))
                using (_logger.AddScope(nameof(migrationsBundle), migrationsBundle.FullName))
                {
                    if (string.IsNullOrEmpty(request.InitialCatalog)) throw new ArgumentNullException(nameof(request.InitialCatalog));
                    if (string.IsNullOrEmpty(request.MigrationName)) throw new ArgumentNullException(nameof(request.MigrationName));
                    if (string.IsNullOrEmpty(request.MigrationsAssemblyPath)) throw new ArgumentNullException(nameof(request.MigrationsAssemblyPath));
                    if (string.IsNullOrEmpty(request.StackName)) throw new ArgumentNullException(nameof(request.StackName));
                    
                    try
                    {
                        _sqlConnectionStringBuilder.InitialCatalog = request.InitialCatalog;
                        switch (request.RequestType)
                        {
                            case CloudFormationRequest.RequestTypeEnum.Create:

                                try
                                {
                                    RunEfBundle(lambdaContext, migrationsBundle, request.MigrationName);

                                    _logger.LogInformation("Worked");

                                    return await CloudFormationResponse.CompleteCloudFormationResponse(CloudFormationResponse.StatusEnum.Success, request, lambdaContext);
                                }
                                catch (Exception e)
                                {
                                    _logger.LogError(e, e.Message);
                                    return await CloudFormationResponse.CompleteCloudFormationResponse(CloudFormationResponse.StatusEnum.Failed, request, lambdaContext, e.Message);
                                }


                            case CloudFormationRequest.RequestTypeEnum.Delete:
                            {
                                using var cloudFormationClient = new AmazonCloudFormationClient();
                                var describeStacksAsync = await cloudFormationClient.DescribeStacksAsync(new DescribeStacksRequest() {StackName = request.StackName});
                                var stackStatus = describeStacksAsync.Stacks.Single().StackStatus;
                                using (_logger.AddScope(nameof(stackStatus), stackStatus))
                                {
                                    var isRollingBack = stackStatus.IsRollingBack();
                                    using (_logger.AddScope(nameof(isRollingBack), isRollingBack))
                                    {
                                        if (!isRollingBack)
                                        {
                                            _logger.LogInformation("Not rolling back, so I'm just going to allow delete without any action");
                                            return await CloudFormationResponse.CompleteCloudFormationResponse(CloudFormationResponse.StatusEnum.Success, request, lambdaContext);
                                        }

                                        var dbContextOptionsBuilder = new DbContextOptionsBuilder();
                                        dbContextOptionsBuilder.UseSqlServer(_sqlConnectionStringBuilder.ConnectionString);
                                        var _dbContext = new DbContext(dbContextOptionsBuilder.Options);
                                        var applied = await _dbContext.Database.GetAppliedMigrationsAsync();
                                        var appliedEnumerated = applied as string[] ?? applied.ToArray();
                                        using (_logger.AddScope(nameof(applied), string.Join(',', appliedEnumerated)))
                                        {
                                            var indexOf = Array.IndexOf(appliedEnumerated,request.MigrationName) - 1;
                                            _logger.LogInformation($"{nameof(indexOf)}={{0}}", indexOf);

                                            if (indexOf < 0)
                                            {
                                                break;
                                            }

                                            var migrateBackTo = appliedEnumerated.ToList()[indexOf];
                                            _logger.LogInformation($"{nameof(migrateBackTo)}={{0}}", migrateBackTo);
                                            using (_logger.AddScope(nameof(migrateBackTo), migrateBackTo))
                                            {
                                                RunEfBundle(lambdaContext, migrationsBundle, migrateBackTo);
                                            }
                                        }

                                    }
                                }

                                break;
                            }

                            default:
                                throw new NotSupportedException(request.RequestType.ToString());
                        }

                        return await CloudFormationResponse.CompleteCloudFormationResponse(CloudFormationResponse.StatusEnum.Success, request, lambdaContext);

                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, e.Message);
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                using (_logger.AddScope(nameof(_sqlConnectionStringBuilder),_sqlConnectionStringBuilder.ConnectionString))
                {
                    _logger.LogError(e, e.Message);

                }                
                return await CloudFormationResponse.CompleteCloudFormationResponse(CloudFormationResponse.StatusEnum.Failed, request, lambdaContext, e.Message);
            }

        }
    }

    private void RunEfBundle(ILambdaContext lambdaContext, FileInfo migrationsBundle, string requestMigrationName)
    {
        using (_logger.AddMember())
        using (_logger.AddScope(nameof(requestMigrationName), requestMigrationName))
        using (_logger.AddScope(nameof(migrationsBundle), migrationsBundle.FullName))
        {
            var start = new ProcessStartInfo(migrationsBundle.FullName)
            {
                WorkingDirectory = (migrationsBundle.Directory.FullName + "/").Replace("//", "/"),
                UseShellExecute = false,
                CreateNoWindow = true,
                ArgumentList =
                {
                    requestMigrationName,
                    "--connection",
                    _sqlConnectionStringBuilder.ConnectionString,
                    "--verbose"
                },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            using (_logger.AddScope(nameof(start.ArgumentList),string.Join(',', start.ArgumentList)))
            {
                _logger.LogInformation("Starting...");
                var process = Process.Start(start);
                ArgumentNullException.ThrowIfNull(process, nameof(process));
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                _logger.LogInformation("Started...");
                var totalMilliseconds = (int)lambdaContext.RemainingTime.Subtract(TimeSpan.FromSeconds(5)).TotalMilliseconds;

                process.OutputDataReceived += (sender, args) => { _logger.LogInformation(args.Data); };
                process.ErrorDataReceived += (sender, args) => { _logger.LogError(args.Data); };

                process.WaitForExit(totalMilliseconds);

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException($"{nameof(process.ExitCode)}!=0 ({process.ExitCode})");
                }

            }
        }
    }


    private async Task<FileInfo> GetFileFromDataArtifacts(string requestMigrationsAssemblyPath, string fileToRetrieve)
    {
        var assemblyPathS3Parts = requestMigrationsAssemblyPath.Split('/');
        var efBundleArchiveFileName = assemblyPathS3Parts.Last();

        var archiveDownloadDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), efBundleArchiveFileName, "archive"));
        var executeFromDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), efBundleArchiveFileName, "execute"));
        executeFromDirectory.Create();
        var finalFile = new FileInfo(Path.Combine(executeFromDirectory.FullName, efBundleArchiveFileName));

        using (_logger.AddMember())
        using (_logger.AddScope(nameof(requestMigrationsAssemblyPath), requestMigrationsAssemblyPath))
        using (_logger.AddScope(nameof(fileToRetrieve), fileToRetrieve))
        using (_logger.AddScope(nameof(assemblyPathS3Parts), string.Join(',', assemblyPathS3Parts)))
        using (_logger.AddScope(nameof(efBundleArchiveFileName), efBundleArchiveFileName))
        using (_logger.AddScope(nameof(finalFile), finalFile.FullName))
        {
            try
            {
                if (!finalFile.Exists)
                {
                    var archiveFile = new FileInfo(Path.Combine(archiveDownloadDirectory.FullName, assemblyPathS3Parts.Last()));

                    using (_logger.AddScope(nameof(archiveFile), archiveFile.FullName))
                    using (_logger.AddScope($"{archiveFile}.{archiveFile.Exists}", archiveFile.Exists))
                    {
                        if (!archiveFile.Exists)
                        {
                            await this.DownloadMigrationsAssembly(requestMigrationsAssemblyPath, archiveFile);
                        }

                        using (_logger.BeginScope("ExtractToDirectory"))
                        {
                            FileInfo fileToCopy;
                            try
                            {
                                System.IO.Compression.ZipFile.ExtractToDirectory(archiveFile.FullName, archiveDownloadDirectory.FullName);
                                archiveFile.Delete();

                                fileToCopy = new FileInfo(Path.Combine(archiveDownloadDirectory.FullName, fileToRetrieve));
                                ArgumentNullException.ThrowIfNull(fileToCopy, nameof(fileToCopy));
                                ArgumentNullException.ThrowIfNull(fileToCopy.Directory, nameof(fileToCopy.Directory));

                                if (!fileToCopy.Exists)
                                {
                                    throw new FileNotFoundException("not found", fileToCopy.FullName);
                                }
                            }
                            catch (Exception e)
                            {
                                _logger.LogWarning(e.ToString());
                                fileToCopy = archiveFile;
                            }

                            System.IO.File.Move(fileToCopy.FullName, finalFile.FullName);

                            _logger.LogInformation("ExtractedToDirectory");
                        }
                    }
                    using (_logger.BeginScope("GetFileSystemEntry"))
                    {
                        try
                        {
                            var unixFileSystemInfo = Mono.Unix.UnixFileSystemInfo.GetFileSystemEntry(finalFile.FullName);
                            unixFileSystemInfo.FileAccessPermissions = FileAccessPermissions.AllPermissions;

                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, e.Message);
                            throw;
                        }
                    }

                    return finalFile;

                }
            }
            catch (System.IO.IOException ioException) when (ioException.Message.Contains("already exists."))
            {
                _logger.LogError(ioException, "Weird");
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }

            return finalFile;
        }
    }

    private Task DownloadMigrationsAssembly(string requestMigrationsAssemblyPath, FileInfo fileInfo)
    {
        using (_logger.AddMember())
        using (_logger.AddScope(nameof(requestMigrationsAssemblyPath), requestMigrationsAssemblyPath))
        using (_logger.AddScope(nameof(fileInfo), fileInfo.FullName))
        {
            try
            {
                fileInfo.Directory!.Create();
                
                return _transferUtility.DownloadAsync(fileInfo, requestMigrationsAssemblyPath);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }
    }
}
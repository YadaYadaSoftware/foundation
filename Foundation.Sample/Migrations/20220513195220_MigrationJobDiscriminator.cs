using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using Foundation.Annotations;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Sample.Migrations;

#nullable disable

namespace BubbleBoy.Data.Migrations
{

    [MigrationFunction(MigrationFunction = typeof(MigrationFunctions), MigrationMethod = nameof(MigrationFunctions.MyMigrator), Branch="@Branch", DependsOn = "Restore")]
    public partial class MigrationJobDiscriminator : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Discriminator",
                schema: "Metadata",
                table: "Job",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ShipmentId",
                schema: "Metadata",
                table: "Job",
                type: "uniqueidentifier",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                schema: "Metadata",
                table: "Job");

            migrationBuilder.DropColumn(
                name: "ShipmentId",
                schema: "Metadata",
                table: "Job");
        }
    }
}

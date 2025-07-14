using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TickTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangepropertiesofAssetsentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Assets");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Assets",
                newName: "Symbol");

            migrationBuilder.RenameColumn(
                name: "Exchange",
                table: "Assets",
                newName: "Kind");

            migrationBuilder.AddColumn<string>(
                name: "BaseCurrency",
                table: "Assets",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Assets",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "TickSize",
                table: "Assets",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseCurrency",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "TickSize",
                table: "Assets");

            migrationBuilder.RenameColumn(
                name: "Symbol",
                table: "Assets",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Kind",
                table: "Assets",
                newName: "Exchange");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Assets",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace User.Management.API.Migrations
{
    /// <inheritdoc />
    public partial class RolesSeeded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "068b8518-b271-4091-8e29-a0570dca0764");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0daced03-0ef1-42eb-be1b-9698e699430f");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b2ea27fd-416b-4faf-a68d-8192ff0a5b16");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1981a941-97c3-425e-814e-f7add78a7676", "1", "Admin", "Admin" },
                    { "6e9003fd-9fd8-4b87-9a62-70cd9b685c38", "3", "Hr", "Hr" },
                    { "c8478edf-d2b6-4870-b39e-e66daafee702", "2", "User", "User" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1981a941-97c3-425e-814e-f7add78a7676");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "6e9003fd-9fd8-4b87-9a62-70cd9b685c38");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c8478edf-d2b6-4870-b39e-e66daafee702");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "068b8518-b271-4091-8e29-a0570dca0764", "1", "Admin", "Admin" },
                    { "0daced03-0ef1-42eb-be1b-9698e699430f", "3", "Hr", "Hr" },
                    { "b2ea27fd-416b-4faf-a68d-8192ff0a5b16", "2", "User", "User" }
                });
        }
    }
}

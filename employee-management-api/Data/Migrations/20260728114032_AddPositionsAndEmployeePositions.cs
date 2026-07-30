using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPositionsAndEmployeePositions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IS_DELETED",
                table: "USERS",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IS_DELETED",
                table: "EMPLOYEES",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IS_DELETED",
                table: "DEPARTMENTS",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "POSITIONS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NAME_EN = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NAME_AR = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IS_DELETED = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CREATED_BY = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CREATION_DATE = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    MODIFIED_BY = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MODIFICATION_DATE = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POSITIONS", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EMPLOYEE_POSITIONS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EMPLOYEE_ID = table.Column<int>(type: "int", nullable: false),
                    POSITION_ID = table.Column<int>(type: "int", nullable: false),
                    START_DATE = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    END_DATE = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IS_DELETED = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CREATED_BY = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CREATION_DATE = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    MODIFIED_BY = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MODIFICATION_DATE = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMPLOYEE_POSITIONS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EMPLOYEE_POSITIONS_EMPLOYEES_EMPLOYEE_ID",
                        column: x => x.EMPLOYEE_ID,
                        principalTable: "EMPLOYEES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EMPLOYEE_POSITIONS_POSITIONS_POSITION_ID",
                        column: x => x.POSITION_ID,
                        principalTable: "POSITIONS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_EMPLOYEE_POSITIONS_EMPLOYEE_ID",
                table: "EMPLOYEE_POSITIONS",
                column: "EMPLOYEE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMPLOYEE_POSITIONS_POSITION_ID",
                table: "EMPLOYEE_POSITIONS",
                column: "POSITION_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EMPLOYEE_POSITIONS");

            migrationBuilder.DropTable(
                name: "POSITIONS");

            migrationBuilder.DropColumn(
                name: "IS_DELETED",
                table: "USERS");

            migrationBuilder.DropColumn(
                name: "IS_DELETED",
                table: "EMPLOYEES");

            migrationBuilder.DropColumn(
                name: "IS_DELETED",
                table: "DEPARTMENTS");
        }
    }
}

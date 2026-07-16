using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace employee_management_api.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeDepartmentRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DEPARTMENT_ID",
                table: "EMPLOYEES",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EMPLOYEES_DEPARTMENT_ID",
                table: "EMPLOYEES",
                column: "DEPARTMENT_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_EMPLOYEES_DEPARTMENTS_DEPARTMENT_ID",
                table: "EMPLOYEES",
                column: "DEPARTMENT_ID",
                principalTable: "DEPARTMENTS",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EMPLOYEES_DEPARTMENTS_DEPARTMENT_ID",
                table: "EMPLOYEES");

            migrationBuilder.DropIndex(
                name: "IX_EMPLOYEES_DEPARTMENT_ID",
                table: "EMPLOYEES");

            migrationBuilder.DropColumn(
                name: "DEPARTMENT_ID",
                table: "EMPLOYEES");
        }
    }
}

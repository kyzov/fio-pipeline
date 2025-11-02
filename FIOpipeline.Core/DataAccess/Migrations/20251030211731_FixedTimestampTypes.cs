using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FIOpipeline.Core.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixedTimestampTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SecondName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BirthdayDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Sex = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ValidTo = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "'9999-12-31 23:59:59'::timestamp"),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                    table.UniqueConstraint("AK_Persons_Id_ValidFrom", x => new { x.Id, x.ValidFrom });
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PersonId = table.Column<int>(type: "integer", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ValidTo = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "'9999-12-31 23:59:59'::timestamp"),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.UniqueConstraint("AK_Addresses_Id_ValidFrom", x => new { x.Id, x.ValidFrom });
                    table.ForeignKey(
                        name: "FK_Addresses_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Emails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PersonId = table.Column<int>(type: "integer", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ValidTo = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "'9999-12-31 23:59:59'::timestamp"),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emails", x => x.Id);
                    table.UniqueConstraint("AK_Emails_Id_ValidFrom", x => new { x.Id, x.ValidFrom });
                    table.ForeignKey(
                        name: "FK_Emails_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Phones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Value = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PersonId = table.Column<int>(type: "integer", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ValidTo = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "'9999-12-31 23:59:59'::timestamp"),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phones", x => x.Id);
                    table.UniqueConstraint("AK_Phones_Id_ValidFrom", x => new { x.Id, x.ValidFrom });
                    table.ForeignKey(
                        name: "FK_Phones_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_PersonId",
                table: "Addresses",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_ValidFrom",
                table: "Addresses",
                column: "ValidFrom");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_ValidTo",
                table: "Addresses",
                column: "ValidTo");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_PersonId",
                table: "Emails",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_ValidFrom",
                table: "Emails",
                column: "ValidFrom");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_ValidTo",
                table: "Emails",
                column: "ValidTo");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_IsCurrent",
                table: "Persons",
                column: "IsCurrent");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_ValidFrom",
                table: "Persons",
                column: "ValidFrom");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_ValidTo",
                table: "Persons",
                column: "ValidTo");

            migrationBuilder.CreateIndex(
                name: "IX_Phones_PersonId",
                table: "Phones",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Phones_ValidFrom",
                table: "Phones",
                column: "ValidFrom");

            migrationBuilder.CreateIndex(
                name: "IX_Phones_ValidTo",
                table: "Phones",
                column: "ValidTo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "Emails");

            migrationBuilder.DropTable(
                name: "Phones");

            migrationBuilder.DropTable(
                name: "Persons");
        }
    }
}

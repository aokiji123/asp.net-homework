﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP_P15.Migrations
{
    /// <inheritdoc />
    public partial class TimeStamp2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Timestamp",
                table: "Feedbacks",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "Feedbacks");
        }
    }
}

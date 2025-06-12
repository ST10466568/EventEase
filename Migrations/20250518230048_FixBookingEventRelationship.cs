using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VenueBooking.Migrations
{
    /// <inheritdoc />
    public partial class FixBookingEventRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Events_EventId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Events_EventId1",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_EventId1",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "EventId1",
                table: "Bookings");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Events_EventId",
                table: "Bookings",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Events_EventId",
                table: "Bookings");

            migrationBuilder.AddColumn<int>(
                name: "EventId1",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_EventId1",
                table: "Bookings",
                column: "EventId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Events_EventId",
                table: "Bookings",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Events_EventId1",
                table: "Bookings",
                column: "EventId1",
                principalTable: "Events",
                principalColumn: "EventId");
        }
    }
}

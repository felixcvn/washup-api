using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WashUpAPIFix.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOrderDateFromLaundryOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "laundryservices",
                columns: table => new
                {
                    laundryserviceid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_laundryservices", x => x.laundryserviceid);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    userid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    passwordhash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.userid);
                });

            migrationBuilder.CreateTable(
                name: "laundryorders",
                columns: table => new
                {
                    laundryorderid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    courierid = table.Column<int>(type: "integer", nullable: true),
                    laundryserviceid = table.Column<int>(type: "integer", nullable: false),
                    pickupaddress = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_laundryorders", x => x.laundryorderid);
                    table.ForeignKey(
                        name: "FK_laundryorders_laundryservices_laundryserviceid",
                        column: x => x.laundryserviceid,
                        principalTable: "laundryservices",
                        principalColumn: "laundryserviceid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_laundryorders_users_courierid",
                        column: x => x.courierid,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_laundryorders_users_userid",
                        column: x => x.userid,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "orderdetails",
                columns: table => new
                {
                    laundryorderid = table.Column<int>(type: "integer", nullable: false),
                    laundryserviceid = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orderdetails", x => new { x.laundryorderid, x.laundryserviceid });
                    table.ForeignKey(
                        name: "FK_orderdetails_laundryorders_laundryorderid",
                        column: x => x.laundryorderid,
                        principalTable: "laundryorders",
                        principalColumn: "laundryorderid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_orderdetails_laundryservices_laundryserviceid",
                        column: x => x.laundryserviceid,
                        principalTable: "laundryservices",
                        principalColumn: "laundryserviceid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    paymentid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    laundryorderid = table.Column<int>(type: "integer", nullable: false),
                    Method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    paidat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    paymentproofurl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.paymentid);
                    table.ForeignKey(
                        name: "FK_payments_laundryorders_laundryorderid",
                        column: x => x.laundryorderid,
                        principalTable: "laundryorders",
                        principalColumn: "laundryorderid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ratings",
                columns: table => new
                {
                    ratingid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    orderid = table.Column<int>(type: "integer", nullable: false),
                    score = table.Column<int>(type: "integer", nullable: false),
                    comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ratedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ratings", x => x.ratingid);
                    table.ForeignKey(
                        name: "FK_ratings_laundryorders_orderid",
                        column: x => x.orderid,
                        principalTable: "laundryorders",
                        principalColumn: "laundryorderid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ratings_users_userid",
                        column: x => x.userid,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_laundryorders_courierid",
                table: "laundryorders",
                column: "courierid");

            migrationBuilder.CreateIndex(
                name: "IX_laundryorders_laundryserviceid",
                table: "laundryorders",
                column: "laundryserviceid");

            migrationBuilder.CreateIndex(
                name: "IX_laundryorders_userid",
                table: "laundryorders",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_orderdetails_laundryserviceid",
                table: "orderdetails",
                column: "laundryserviceid");

            migrationBuilder.CreateIndex(
                name: "IX_payments_laundryorderid",
                table: "payments",
                column: "laundryorderid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ratings_orderid",
                table: "ratings",
                column: "orderid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ratings_userid",
                table: "ratings",
                column: "userid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "orderdetails");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "ratings");

            migrationBuilder.DropTable(
                name: "laundryorders");

            migrationBuilder.DropTable(
                name: "laundryservices");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}

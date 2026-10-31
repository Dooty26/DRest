using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Restaurant.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE SCHEMA IF NOT EXISTS extensions;
                CREATE EXTENSION IF NOT EXISTS btree_gist WITH SCHEMA extensions;
                SET LOCAL search_path = public, extensions;
                """);

            migrationBuilder.CreateTable(
                name: "app_settings",
                columns: table => new
                {
                    key = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_settings", x => x.key);
                });

            migrationBuilder.CreateTable(
                name: "areas",
                columns: table => new
                {
                    area_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    floor_no = table.Column<int>(type: "integer", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    floor_plan_image_url = table.Column<string>(type: "text", nullable: true),
                    floor_plan_image_path = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_areas", x => x.area_id);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    log_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: true),
                    action = table.Column<string>(type: "text", nullable: false),
                    entity = table.Column<string>(type: "text", nullable: false),
                    entity_id = table.Column<string>(type: "text", nullable: true),
                    detail = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_logs", x => x.log_id);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    customer_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    full_name = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: true),
                    zalo_user_id = table.Column<string>(type: "text", nullable: true),
                    is_vip = table.Column<bool>(type: "boolean", nullable: false),
                    total_visits = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customers", x => x.customer_id);
                });

            migrationBuilder.CreateTable(
                name: "daily_counters",
                columns: table => new
                {
                    counter_date = table.Column<DateOnly>(type: "date", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_daily_counters", x => new { x.counter_date, x.name });
                });

            migrationBuilder.CreateTable(
                name: "ingredients",
                columns: table => new
                {
                    ingredient_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    base_unit = table.Column<string>(type: "text", nullable: false),
                    display_unit = table.Column<string>(type: "text", nullable: false),
                    display_factor = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
                    stock_qty = table.Column<decimal>(type: "numeric(14,3)", precision: 14, scale: 3, nullable: false),
                    min_stock = table.Column<decimal>(type: "numeric(14,3)", precision: 14, scale: 3, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ingredients", x => x.ingredient_id);
                    table.CheckConstraint("ck_ingredients_base_unit", "base_unit IN ('g','ml','pcs')");
                });

            migrationBuilder.CreateTable(
                name: "notification_logs",
                columns: table => new
                {
                    log_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    reservation_id = table.Column<long>(type: "bigint", nullable: true),
                    request_id = table.Column<long>(type: "bigint", nullable: true),
                    channel = table.Column<string>(type: "text", nullable: false),
                    direction = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "text", nullable: true),
                    template_id = table.Column<string>(type: "text", nullable: true),
                    body = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    provider_msg_id = table.Column<string>(type: "text", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_logs", x => x.log_id);
                    table.CheckConstraint("ck_notification_channel", "channel IN ('ZALO_ZNS','ZALO_OA','MOCK')");
                    table.CheckConstraint("ck_notification_direction", "direction IN ('OUT','IN')");
                    table.CheckConstraint("ck_notification_status", "status IN ('SENT','FAILED','RECEIVED')");
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.role_id);
                    table.CheckConstraint("ck_roles_code", "code IN ('MANAGER','CASHIER','WAITER','KITCHEN')");
                });

            migrationBuilder.CreateTable(
                name: "tables",
                columns: table => new
                {
                    table_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    area_id = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    seats = table.Column<int>(type: "integer", nullable: false),
                    extra_chairs_max = table.Column<int>(type: "integer", nullable: false),
                    view_type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    status_since = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    pos_x = table.Column<int>(type: "integer", nullable: false),
                    pos_y = table.Column<int>(type: "integer", nullable: false),
                    width = table.Column<int>(type: "integer", nullable: false),
                    height = table.Column<int>(type: "integer", nullable: false),
                    shape = table.Column<string>(type: "text", nullable: false),
                    qr_secret = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tables", x => x.table_id);
                    table.CheckConstraint("ck_tables_seats", "seats > 0");
                    table.CheckConstraint("ck_tables_shape", "shape IN ('RECT','ROUND')");
                    table.CheckConstraint("ck_tables_status", "status IN ('AVAILABLE','OCCUPIED','CLEANING','HELD')");
                    table.CheckConstraint("ck_tables_view_type", "view_type IN ('STANDARD','WINDOW','BALCONY','VIP','OUTDOOR')");
                    table.ForeignKey(
                        name: "fk_tables_areas_area_id",
                        column: x => x.area_id,
                        principalTable: "areas",
                        principalColumn: "area_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "menu_items",
                columns: table => new
                {
                    item_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    price = table.Column<decimal>(type: "numeric(12,0)", precision: 12, scale: 0, nullable: false),
                    image_url = table.Column<string>(type: "text", nullable: true),
                    image_path = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_available = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_menu_items", x => x.item_id);
                    table.CheckConstraint("ck_menu_items_price", "price >= 0");
                    table.ForeignKey(
                        name: "fk_menu_items_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vouchers",
                columns: table => new
                {
                    voucher_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    source = table.Column<string>(type: "text", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    redeemed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    redeemed_invoice_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vouchers", x => x.voucher_id);
                    table.CheckConstraint("ck_vouchers_source", "source IN ('DEPOSIT_REFUND','APOLOGY','PROMO')");
                    table.CheckConstraint("ck_vouchers_type", "type IN ('FIXED','PERCENT')");
                    table.ForeignKey(
                        name: "fk_vouchers_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    full_name = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    failed_login_count = table.Column<int>(type: "integer", nullable: false),
                    locked_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.user_id);
                    table.ForeignKey(
                        name: "fk_users_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reservations",
                columns: table => new
                {
                    reservation_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "text", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: true),
                    table_id = table.Column<int>(type: "integer", nullable: false),
                    guest_name = table.Column<string>(type: "text", nullable: false),
                    guest_phone = table.Column<string>(type: "text", nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    occupancy_minutes = table.Column<int>(type: "integer", nullable: false),
                    occupied_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    party_size = table.Column<int>(type: "integer", nullable: false),
                    view_type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    allergies = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    occasion = table.Column<string>(type: "text", nullable: true),
                    baby_chair = table.Column<bool>(type: "boolean", nullable: false),
                    needs_extra_chairs = table.Column<bool>(type: "boolean", nullable: false),
                    deposit_amount = table.Column<decimal>(type: "numeric(12,0)", precision: 12, scale: 0, nullable: false),
                    deposit_status = table.Column<string>(type: "text", nullable: false),
                    deposit_paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deposit_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    source = table.Column<string>(type: "text", nullable: false),
                    apology_discount_pct = table.Column<int>(type: "integer", nullable: false),
                    reschedule_count = table.Column<int>(type: "integer", nullable: false),
                    idempotency_key = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reservations", x => x.reservation_id);
                    table.CheckConstraint("ck_reservations_deposit_status", "deposit_status IN ('NONE','PENDING','PAID','REFUNDED','FORFEITED','CONVERTED_TO_VOUCHER','APPLIED')");
                    table.CheckConstraint("ck_reservations_occupancy", "occupancy_minutes > 0");
                    table.CheckConstraint("ck_reservations_party", "party_size > 0");
                    table.CheckConstraint("ck_reservations_source", "source IN ('WEB','STAFF')");
                    table.CheckConstraint("ck_reservations_status", "status IN ('PENDING_DEPOSIT','CONFIRMED','CHECKED_IN','COMPLETED','CANCELLED','NO_SHOW','EXPIRED')");
                    table.ForeignKey(
                        name: "fk_reservations_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_reservations_tables_table_id",
                        column: x => x.table_id,
                        principalTable: "tables",
                        principalColumn: "table_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "waitlist_entries",
                columns: table => new
                {
                    entry_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "text", nullable: false),
                    party_size = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    table_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    notified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    seated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_waitlist_entries", x => x.entry_id);
                    table.CheckConstraint("ck_waitlist_party", "party_size > 0");
                    table.CheckConstraint("ck_waitlist_status", "status IN ('WAITING','NOTIFIED','SEATED','CANCELLED')");
                    table.ForeignKey(
                        name: "fk_waitlist_entries_tables_table_id",
                        column: x => x.table_id,
                        principalTable: "tables",
                        principalColumn: "table_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "recipes",
                columns: table => new
                {
                    item_id = table.Column<int>(type: "integer", nullable: false),
                    ingredient_id = table.Column<int>(type: "integer", nullable: false),
                    qty = table.Column<decimal>(type: "numeric(14,3)", precision: 14, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recipes", x => new { x.item_id, x.ingredient_id });
                    table.CheckConstraint("ck_recipes_qty", "qty > 0");
                    table.ForeignKey(
                        name: "fk_recipes_ingredients_ingredient_id",
                        column: x => x.ingredient_id,
                        principalTable: "ingredients",
                        principalColumn: "ingredient_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_recipes_menu_items_item_id",
                        column: x => x.item_id,
                        principalTable: "menu_items",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    order_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    table_id = table.Column<int>(type: "integer", nullable: false),
                    reservation_id = table.Column<long>(type: "bigint", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    party_size = table.Column<int>(type: "integer", nullable: false),
                    opened_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    opened_by_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.order_id);
                    table.CheckConstraint("ck_orders_status", "status IN ('OPEN','BILL_REQUESTED','PAID','CANCELLED')");
                    table.ForeignKey(
                        name: "fk_orders_reservations_reservation_id",
                        column: x => x.reservation_id,
                        principalTable: "reservations",
                        principalColumn: "reservation_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_orders_tables_table_id",
                        column: x => x.table_id,
                        principalTable: "tables",
                        principalColumn: "table_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_orders_users_opened_by_user_id",
                        column: x => x.opened_by_user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reservation_wishlists",
                columns: table => new
                {
                    wishlist_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    reservation_id = table.Column<long>(type: "bigint", nullable: false),
                    target_view_type = table.Column<string>(type: "text", nullable: false),
                    target_table_id = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reservation_wishlists", x => x.wishlist_id);
                    table.ForeignKey(
                        name: "fk_reservation_wishlists_reservations_reservation_id",
                        column: x => x.reservation_id,
                        principalTable: "reservations",
                        principalColumn: "reservation_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_reservation_wishlists_tables_target_table_id",
                        column: x => x.target_table_id,
                        principalTable: "tables",
                        principalColumn: "table_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "table_switch_requests",
                columns: table => new
                {
                    request_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    res_id = table.Column<long>(type: "bigint", nullable: false),
                    from_table_id = table.Column<int>(type: "integer", nullable: false),
                    to_table_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    attempt_no = table.Column<int>(type: "integer", nullable: false),
                    rank_score = table.Column<string>(type: "text", nullable: true),
                    message_text = table.Column<string>(type: "text", nullable: true),
                    offered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    escalated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    escalation_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resolved_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    resolution = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_table_switch_requests", x => x.request_id);
                    table.CheckConstraint("ck_switch_status", "status IN ('PENDING_CUSTOMER','ACCEPTED','DECLINED','ESCALATED','STAFF_ACCEPTED','STAFF_KEPT_OLD','STAFF_NO_ANSWER','AUTO_RELEASED')");
                    table.ForeignKey(
                        name: "fk_table_switch_requests_reservations_res_id",
                        column: x => x.res_id,
                        principalTable: "reservations",
                        principalColumn: "reservation_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_table_switch_requests_tables_from_table_id",
                        column: x => x.from_table_id,
                        principalTable: "tables",
                        principalColumn: "table_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_table_switch_requests_tables_to_table_id",
                        column: x => x.to_table_id,
                        principalTable: "tables",
                        principalColumn: "table_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_table_switch_requests_users_resolved_by_user_id",
                        column: x => x.resolved_by_user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    invoice_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    invoice_no = table.Column<string>(type: "text", nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(12,0)", precision: 12, scale: 0, nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric(12,0)", precision: 12, scale: 0, nullable: false),
                    discount_reason = table.Column<string>(type: "text", nullable: true),
                    vat_amount = table.Column<decimal>(type: "numeric(12,0)", precision: 12, scale: 0, nullable: false),
                    deposit_applied = table.Column<decimal>(type: "numeric(12,0)", precision: 12, scale: 0, nullable: false),
                    net_amount = table.Column<decimal>(type: "numeric(12,0)", precision: 12, scale: 0, nullable: false),
                    refund_due = table.Column<decimal>(type: "numeric(12,0)", precision: 12, scale: 0, nullable: false),
                    amount_tendered = table.Column<decimal>(type: "numeric(12,0)", precision: 12, scale: 0, nullable: true),
                    payment_method = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    voucher_id = table.Column<long>(type: "bigint", nullable: true),
                    cashier_id = table.Column<int>(type: "integer", nullable: true),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    stock_deducted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invoices", x => x.invoice_id);
                    table.CheckConstraint("ck_invoices_payment_method", "payment_method IS NULL OR payment_method IN ('VIETQR','CASH','CARD','TRANSFER')");
                    table.CheckConstraint("ck_invoices_status", "status IN ('DRAFT','PAID','VOID')");
                    table.ForeignKey(
                        name: "fk_invoices_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "order_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_invoices_users_cashier_id",
                        column: x => x.cashier_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_invoices_vouchers_voucher_id",
                        column: x => x.voucher_id,
                        principalTable: "vouchers",
                        principalColumn: "voucher_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "order_details",
                columns: table => new
                {
                    detail_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    item_id = table.Column<int>(type: "integer", nullable: false),
                    qty = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(12,0)", precision: 12, scale: 0, nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    allergy_note = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    source = table.Column<string>(type: "text", nullable: false),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    ordered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    kitchen_printed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    idempotency_key = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_details", x => x.detail_id);
                    table.CheckConstraint("ck_order_details_qty", "qty > 0");
                    table.CheckConstraint("ck_order_details_source", "source IN ('QR','STAFF')");
                    table.CheckConstraint("ck_order_details_status", "status IN ('NEW','COOKING','READY','SERVED','CANCELLED','OUT_OF_STOCK')");
                    table.ForeignKey(
                        name: "fk_order_details_menu_items_item_id",
                        column: x => x.item_id,
                        principalTable: "menu_items",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_order_details_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "order_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_order_details_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "table_merges",
                columns: table => new
                {
                    merge_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    primary_table_id = table.Column<int>(type: "integer", nullable: false),
                    merged_table_id = table.Column<int>(type: "integer", nullable: false),
                    merged_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    unmerged_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_table_merges", x => x.merge_id);
                    table.ForeignKey(
                        name: "fk_table_merges_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "order_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_table_merges_tables_merged_table_id",
                        column: x => x.merged_table_id,
                        principalTable: "tables",
                        principalColumn: "table_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_table_merges_tables_primary_table_id",
                        column: x => x.primary_table_id,
                        principalTable: "tables",
                        principalColumn: "table_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "switch_request_events",
                columns: table => new
                {
                    event_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    request_id = table.Column<long>(type: "bigint", nullable: false),
                    step = table.Column<string>(type: "text", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_switch_request_events", x => x.event_id);
                    table.ForeignKey(
                        name: "fk_switch_request_events_table_switch_requests_request_id",
                        column: x => x.request_id,
                        principalTable: "table_switch_requests",
                        principalColumn: "request_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    payment_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    reservation_id = table.Column<long>(type: "bigint", nullable: true),
                    invoice_id = table.Column<long>(type: "bigint", nullable: true),
                    type = table.Column<string>(type: "text", nullable: false),
                    method = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(12,0)", precision: 12, scale: 0, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    provider = table.Column<string>(type: "text", nullable: true),
                    provider_txn_id = table.Column<string>(type: "text", nullable: true),
                    content_code = table.Column<string>(type: "text", nullable: true),
                    raw_payload = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payments", x => x.payment_id);
                    table.CheckConstraint("ck_payments_status", "status IN ('PENDING','CONFIRMED','FAILED','REVIEW')");
                    table.CheckConstraint("ck_payments_type", "type IN ('DEPOSIT','REFUND','BILL')");
                    table.ForeignKey(
                        name: "fk_payments_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalTable: "invoices",
                        principalColumn: "invoice_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_payments_reservations_reservation_id",
                        column: x => x.reservation_id,
                        principalTable: "reservations",
                        principalColumn: "reservation_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stock_movements",
                columns: table => new
                {
                    movement_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ingredient_id = table.Column<int>(type: "integer", nullable: false),
                    delta = table.Column<decimal>(type: "numeric(14,3)", precision: 14, scale: 3, nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    ref_invoice_id = table.Column<long>(type: "bigint", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_movements", x => x.movement_id);
                    table.CheckConstraint("ck_stock_movements_type", "type IN ('SALE','PURCHASE','ADJUST','WASTE')");
                    table.ForeignKey(
                        name: "fk_stock_movements_ingredients_ingredient_id",
                        column: x => x.ingredient_id,
                        principalTable: "ingredients",
                        principalColumn: "ingredient_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_stock_movements_invoices_ref_invoice_id",
                        column: x => x.ref_invoice_id,
                        principalTable: "invoices",
                        principalColumn: "invoice_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_stock_movements_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_entity",
                table: "audit_logs",
                columns: new[] { "entity", "entity_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_customers_phone",
                table: "customers",
                column: "phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ingredients_name",
                table: "ingredients",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_invoices_cashier_id",
                table: "invoices",
                column: "cashier_id");

            migrationBuilder.CreateIndex(
                name: "ix_invoices_invoice_no",
                table: "invoices",
                column: "invoice_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_invoices_order_id",
                table: "invoices",
                column: "order_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_invoices_voucher_id",
                table: "invoices",
                column: "voucher_id");

            migrationBuilder.CreateIndex(
                name: "ix_menu_items_category_id",
                table: "menu_items",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_nl_phone",
                table: "notification_logs",
                columns: new[] { "phone", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_od_order_status",
                table: "order_details",
                columns: new[] { "order_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_order_details_created_by_user_id",
                table: "order_details",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_details_item_id",
                table: "order_details",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "ux_od_idem",
                table: "order_details",
                columns: new[] { "order_id", "idempotency_key" },
                unique: true,
                filter: "idempotency_key IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_orders_opened_by_user_id",
                table: "orders",
                column: "opened_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_reservation_id",
                table: "orders",
                column: "reservation_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_table_status",
                table: "orders",
                columns: new[] { "table_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ux_orders_one_open_per_table",
                table: "orders",
                column: "table_id",
                unique: true,
                filter: "status IN ('OPEN','BILL_REQUESTED')");

            migrationBuilder.CreateIndex(
                name: "ix_pay_content",
                table: "payments",
                columns: new[] { "content_code", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_payments_invoice_id",
                table: "payments",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_provider_txn_id",
                table: "payments",
                column: "provider_txn_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payments_reservation_id",
                table: "payments",
                column: "reservation_id");

            migrationBuilder.CreateIndex(
                name: "ix_recipes_ingredient_id",
                table: "recipes",
                column: "ingredient_id");

            migrationBuilder.CreateIndex(
                name: "ix_reservation_wishlists_reservation_id",
                table: "reservation_wishlists",
                column: "reservation_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_reservation_wishlists_target_table_id",
                table: "reservation_wishlists",
                column: "target_table_id");

            migrationBuilder.CreateIndex(
                name: "ix_wl_active_view",
                table: "reservation_wishlists",
                column: "target_view_type",
                filter: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_res_phone",
                table: "reservations",
                column: "guest_phone");

            migrationBuilder.CreateIndex(
                name: "ix_res_status_expires",
                table: "reservations",
                columns: new[] { "status", "deposit_expires_at" },
                filter: "status = 'PENDING_DEPOSIT'");

            migrationBuilder.CreateIndex(
                name: "ix_res_table_start",
                table: "reservations",
                columns: new[] { "table_id", "start_time" });

            migrationBuilder.CreateIndex(
                name: "ix_reservations_code",
                table: "reservations",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_reservations_customer_id",
                table: "reservations",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_reservations_idempotency_key",
                table: "reservations",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_roles_code",
                table: "roles",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_stock_movements_created_by",
                table: "stock_movements",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_stock_movements_ingredient_id",
                table: "stock_movements",
                column: "ingredient_id");

            migrationBuilder.CreateIndex(
                name: "ux_sm_sale",
                table: "stock_movements",
                columns: new[] { "ref_invoice_id", "ingredient_id" },
                unique: true,
                filter: "type = 'SALE'");

            migrationBuilder.CreateIndex(
                name: "ix_switch_request_events_request_id",
                table: "switch_request_events",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "ix_table_merges_merged_table_id",
                table: "table_merges",
                column: "merged_table_id");

            migrationBuilder.CreateIndex(
                name: "ix_table_merges_order_id",
                table: "table_merges",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_table_merges_primary_table_id",
                table: "table_merges",
                column: "primary_table_id");

            migrationBuilder.CreateIndex(
                name: "ix_sw_escalated",
                table: "table_switch_requests",
                column: "escalation_expires_at",
                filter: "status = 'ESCALATED'");

            migrationBuilder.CreateIndex(
                name: "ix_sw_pending",
                table: "table_switch_requests",
                column: "expires_at",
                filter: "status = 'PENDING_CUSTOMER'");

            migrationBuilder.CreateIndex(
                name: "ix_table_switch_requests_from_table_id",
                table: "table_switch_requests",
                column: "from_table_id");

            migrationBuilder.CreateIndex(
                name: "ix_table_switch_requests_resolved_by_user_id",
                table: "table_switch_requests",
                column: "resolved_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ux_sw_one_open_per_res",
                table: "table_switch_requests",
                column: "res_id",
                unique: true,
                filter: "status IN ('PENDING_CUSTOMER','ESCALATED')");

            migrationBuilder.CreateIndex(
                name: "ux_sw_one_open_per_table",
                table: "table_switch_requests",
                column: "to_table_id",
                unique: true,
                filter: "status IN ('PENDING_CUSTOMER','ESCALATED')");

            migrationBuilder.CreateIndex(
                name: "ix_tables_area",
                table: "tables",
                columns: new[] { "area_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_tables_code",
                table: "tables",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_role_id",
                table: "users",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                table: "users",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vouchers_code",
                table: "vouchers",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vouchers_customer_id",
                table: "vouchers",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_wait_status",
                table: "waitlist_entries",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_waitlist_entries_table_id",
                table: "waitlist_entries",
                column: "table_id");

            migrationBuilder.Sql("""
                ALTER TABLE public.reservations
                ADD CONSTRAINT ex_reservations_no_overlap
                EXCLUDE USING gist
                (
                    table_id WITH =,
                    tstzrange(start_time, occupied_until, '[]') WITH &&
                )
                WHERE (status IN ('PENDING_DEPOSIT', 'CONFIRMED', 'CHECKED_IN'));
                """);

            migrationBuilder.Sql(DatabaseSecuritySql.LockDownPublicSchema);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_settings");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "daily_counters");

            migrationBuilder.DropTable(
                name: "notification_logs");

            migrationBuilder.DropTable(
                name: "order_details");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "recipes");

            migrationBuilder.DropTable(
                name: "reservation_wishlists");

            migrationBuilder.DropTable(
                name: "stock_movements");

            migrationBuilder.DropTable(
                name: "switch_request_events");

            migrationBuilder.DropTable(
                name: "table_merges");

            migrationBuilder.DropTable(
                name: "waitlist_entries");

            migrationBuilder.DropTable(
                name: "menu_items");

            migrationBuilder.DropTable(
                name: "ingredients");

            migrationBuilder.DropTable(
                name: "invoices");

            migrationBuilder.DropTable(
                name: "table_switch_requests");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "vouchers");

            migrationBuilder.DropTable(
                name: "reservations");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "tables");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "areas");
        }
    }
}

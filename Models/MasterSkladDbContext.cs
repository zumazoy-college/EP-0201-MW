using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace EP_0201_MW.Models;

public partial class MasterSkladDbContext : DbContext
{
    public MasterSkladDbContext()
    {
    }

    public MasterSkladDbContext(DbContextOptions<MasterSkladDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Lease> Leases { get; set; }

    public virtual DbSet<Object> Objects { get; set; }

    public virtual DbSet<PaymentStatus> PaymentStatuses { get; set; }

    public virtual DbSet<Position> Positions { get; set; }

    public virtual DbSet<ProvidedService> ProvidedServices { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Warehouse> Warehouses { get; set; }

    public virtual DbSet<WarehouseStatus> WarehouseStatuses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Строим конфигурацию из файла appsettings.json
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.IdClient).HasName("PK__clients__27D6F212AE4528E4");

            entity.ToTable("clients");

            entity.HasIndex(e => e.PhoneNumber, "UQ__clients__4849DA013E912891").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__clients__AB6E61640272ACAA").IsUnique();

            entity.Property(e => e.IdClient).HasColumnName("ID_client");
            entity.Property(e => e.CompanyName)
                .HasMaxLength(150)
                .HasColumnName("companyName");
            entity.Property(e => e.ContractDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("contractDate");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FirstNamePerson)
                .HasMaxLength(50)
                .HasColumnName("firstNamePerson");
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
            entity.Property(e => e.LastNamePerson)
                .HasMaxLength(50)
                .HasColumnName("lastNamePerson");
            entity.Property(e => e.MiddleNamePerson)
                .HasMaxLength(50)
                .HasColumnName("middleNamePerson");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(11)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("phoneNumber");
            entity.Property(e => e.Requisites).HasColumnName("requisites");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.IdEmployee).HasName("PK__employee__D9846634601DB2BC");

            entity.ToTable("employees");

            entity.HasIndex(e => e.Email, "UQ__employee__AB6E61645A80B084").IsUnique();

            entity.Property(e => e.IdEmployee).HasColumnName("ID_employee");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("firstName");
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("lastName");
            entity.Property(e => e.MiddleName)
                .HasMaxLength(50)
                .HasColumnName("middleName");
            entity.Property(e => e.PositionId).HasColumnName("position_ID");

            entity.HasOne(d => d.Position).WithMany(p => p.Employees)
                .HasForeignKey(d => d.PositionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__employees__posit__4F7CD00D");
        });

        modelBuilder.Entity<Lease>(entity =>
        {
            entity.HasKey(e => e.IdLease).HasName("PK__leases__E18C2CE29253B1C5");

            entity.ToTable("leases");

            entity.HasIndex(e => e.ContractNumber, "UQ__leases__66292B25322D59B7").IsUnique();

            entity.Property(e => e.IdLease).HasColumnName("ID_lease");
            entity.Property(e => e.ClientId).HasColumnName("client_ID");
            entity.Property(e => e.ContractNumber)
                .HasMaxLength(50)
                .HasColumnName("contractNumber");
            entity.Property(e => e.EndDate).HasColumnName("endDate");
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
            entity.Property(e => e.ManagerId).HasColumnName("manager_ID");
            entity.Property(e => e.PstatusId).HasColumnName("pstatus_ID");
            entity.Property(e => e.StartDate).HasColumnName("startDate");
            entity.Property(e => e.TotalPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("totalPrice");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_ID");

            entity.HasOne(d => d.Client).WithMany(p => p.Leases)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__leases__client_I__71D1E811");

            entity.HasOne(d => d.Manager).WithMany(p => p.Leases)
                .HasForeignKey(d => d.ManagerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__leases__manager___73BA3083");

            entity.HasOne(d => d.Pstatus).WithMany(p => p.Leases)
                .HasForeignKey(d => d.PstatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__leases__pstatus___74AE54BC");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.Leases)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__leases__warehous__72C60C4A");
        });

        modelBuilder.Entity<Object>(entity =>
        {
            entity.HasKey(e => e.IdObject).HasName("PK__objects__23E033B6C180F5AA");

            entity.ToTable("objects");

            entity.HasIndex(e => e.Address, "UQ__objects__751C8E54F514048A").IsUnique();

            entity.Property(e => e.IdObject).HasColumnName("ID_object");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Description).HasColumnName("description");
        });

        modelBuilder.Entity<PaymentStatus>(entity =>
        {
            entity.HasKey(e => e.IdPstatus).HasName("PK__payment___F46268D497AD91BE");

            entity.ToTable("payment_statuses");

            entity.HasIndex(e => e.Title, "UQ__payment___E52A1BB327E53861").IsUnique();

            entity.Property(e => e.IdPstatus).HasColumnName("ID_pstatus");
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .HasColumnName("title");
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => e.IdPosition).HasName("PK__position__91FB1AE0DEC740D8");

            entity.ToTable("positions");

            entity.HasIndex(e => e.Title, "UQ__position__E52A1BB3229BA14F").IsUnique();

            entity.Property(e => e.IdPosition).HasColumnName("ID_position");
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .HasColumnName("title");
        });

        modelBuilder.Entity<ProvidedService>(entity =>
        {
            entity.HasKey(e => e.IdProvidedService).HasName("PK__provided__FBA452B026D9E670");

            entity.ToTable("provided_services");

            entity.Property(e => e.IdProvidedService).HasColumnName("ID_provided_service");
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
            entity.Property(e => e.LeaseId).HasColumnName("lease_ID");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1)
                .HasColumnName("quantity");
            entity.Property(e => e.ServiceDate).HasColumnName("serviceDate");
            entity.Property(e => e.ServiceId).HasColumnName("service_ID");

            entity.HasOne(d => d.Lease).WithMany(p => p.ProvidedServices)
                .HasForeignKey(d => d.LeaseId)
                .HasConstraintName("FK__provided___lease__7E37BEF6");

            entity.HasOne(d => d.Service).WithMany(p => p.ProvidedServices)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__provided___servi__7F2BE32F");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRole).HasName("PK__roles__45DFFBDBEB006557");

            entity.ToTable("roles");

            entity.HasIndex(e => e.Title, "UQ__roles__E52A1BB3B8F61CC6").IsUnique();

            entity.Property(e => e.IdRole).HasColumnName("ID_role");
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .HasColumnName("title");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.IdService).HasName("PK__services__3E6EB21F9BD51592");

            entity.ToTable("services");

            entity.Property(e => e.IdService).HasColumnName("ID_service");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasColumnName("title");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("PK__users__D7B4671E07F8B14A");

            entity.ToTable("users");

            entity.HasIndex(e => e.Login, "UQ__users__7838F272CDEEB6C0").IsUnique();

            entity.Property(e => e.IdUser).HasColumnName("ID_user");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_ID");
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
            entity.Property(e => e.Login)
                .HasMaxLength(30)
                .HasColumnName("login");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.RoleId).HasColumnName("role_ID");

            entity.HasOne(d => d.Employee).WithMany(p => p.Users)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__users__employee___5812160E");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__users__role_ID__571DF1D5");
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasKey(e => e.IdWarehouse).HasName("PK__warehous__E5C048BFB14F575C");

            entity.ToTable("warehouses");

            entity.Property(e => e.IdWarehouse).HasColumnName("ID_warehouse");
            entity.Property(e => e.Area)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("area");
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
            entity.Property(e => e.MonthlyPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monthlyPrice");
            entity.Property(e => e.ObjectId).HasColumnName("object_ID");
            entity.Property(e => e.StatusId).HasColumnName("status_ID");
            entity.Property(e => e.WarehouseNumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("warehouse_number");

            entity.HasOne(d => d.Object).WithMany(p => p.Warehouses)
                .HasForeignKey(d => d.ObjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__warehouse__objec__6383C8BA");

            entity.HasOne(d => d.Status).WithMany(p => p.Warehouses)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__warehouse__statu__628FA481");
        });

        modelBuilder.Entity<WarehouseStatus>(entity =>
        {
            entity.HasKey(e => e.IdStatus).HasName("PK__warehous__449BD7E6DFABAF81");

            entity.ToTable("warehouse_statuses");

            entity.HasIndex(e => e.Title, "UQ__warehous__E52A1BB3D1EDEC54").IsUnique();

            entity.Property(e => e.IdStatus).HasColumnName("ID_status");
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .HasColumnName("title");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

using Microsoft.EntityFrameworkCore;
using Core.Entities;

namespace Data.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<EmployeePosition> EmployeePositions { get; set; }
        public DbSet<EmployeeDocument> EmployeeDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // users
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("USERS");
                entity.Property(u => u.Id).HasColumnName("ID");
                entity.Property(u => u.Username).HasColumnName("USERNAME");
                entity.Property(u => u.Name).HasColumnName("NAME");
                entity.Property(u => u.Password).HasColumnName("PASSWORD");
                entity.Property(u => u.Status).HasColumnName("STATUS");
                entity.Property(u => u.IsDeleted).HasColumnName("IS_DELETED");
                entity.Property(u => u.CreatedBy).HasColumnName("CREATED_BY");
                entity.Property(u => u.CreationDate).HasColumnName("CREATION_DATE");
                entity.Property(u => u.ModifiedBy).HasColumnName("MODIFIED_BY");
                entity.Property(u => u.ModificationDate).HasColumnName("MODIFICATION_DATE");

                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasQueryFilter(u => !u.IsDeleted);
            });

            // employees
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("EMPLOYEES");
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.EmployeeNo).HasColumnName("EMPLOYEE_NO");
                entity.Property(e => e.NameEn).HasColumnName("NAME_EN");
                entity.Property(e => e.NameAr).HasColumnName("NAME_AR");
                entity.Property(e => e.Username).HasColumnName("USERNAME");
                entity.Property(e => e.Birthdate).HasColumnName("BIRTHDATE");
                entity.Property(e => e.NationalNo).HasColumnName("NATIONAL_NO");
                entity.Property(e => e.Gender).HasColumnName("GENDER");
                entity.Property(e => e.Status).HasColumnName("STATUS");
                entity.Property(e => e.MobileNumber).HasColumnName("MOBILE_NUMBER");
                entity.Property(e => e.Email).HasColumnName("EMAIL");
                entity.Property(e => e.StartWorkingDate).HasColumnName("START_WORKING_DATE");
                entity.Property(e => e.DepartmentId).HasColumnName("DEPARTMENT_ID");
                entity.Property(e => e.ProfileImagePath).HasColumnName("PROFILE_IMAGE_PATH");
                entity.Property(e => e.IsDeleted).HasColumnName("IS_DELETED");
                entity.Property(e => e.CreatedBy).HasColumnName("CREATED_BY");
                entity.Property(e => e.CreationDate).HasColumnName("CREATION_DATE");
                entity.Property(e => e.ModifiedBy).HasColumnName("MODIFIED_BY");
                entity.Property(e => e.ModificationDate).HasColumnName("MODIFICATION_DATE");

                entity.HasOne<User>()
                      .WithOne()
                      .HasForeignKey<Employee>(e => e.Username)
                      .HasPrincipalKey<User>(u => u.Username)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Department)
                      .WithMany()
                      .HasForeignKey(e => e.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // departments
            modelBuilder.Entity<Department>(entity =>
            {
                entity.ToTable("DEPARTMENTS");
                entity.Property(d => d.Id).HasColumnName("ID");
                entity.Property(d => d.DepartmentCode).HasColumnName("DEPARTMENT_CODE");
                entity.Property(d => d.NameEn).HasColumnName("NAME_EN");
                entity.Property(d => d.NameAr).HasColumnName("NAME_AR");
                entity.Property(d => d.Description).HasColumnName("DESCRIPTION");
                entity.Property(d => d.Status).HasColumnName("STATUS");
                entity.Property(d => d.IsDeleted).HasColumnName("IS_DELETED");
                entity.Property(d => d.CreatedBy).HasColumnName("CREATED_BY");
                entity.Property(d => d.CreationDate).HasColumnName("CREATION_DATE");
                entity.Property(d => d.ModifiedBy).HasColumnName("MODIFIED_BY");
                entity.Property(d => d.ModificationDate).HasColumnName("MODIFICATION_DATE");

                entity.HasIndex(d => d.DepartmentCode).IsUnique();
                entity.HasQueryFilter(d => !d.IsDeleted);
            });

            // positions
            modelBuilder.Entity<Position>(entity =>
            {
                entity.ToTable("POSITIONS");
                entity.Property(p => p.Id).HasColumnName("ID");
                entity.Property(p => p.NameEn).HasColumnName("NAME_EN");
                entity.Property(p => p.NameAr).HasColumnName("NAME_AR");
                entity.Property(p => p.IsDeleted).HasColumnName("IS_DELETED");
                entity.Property(p => p.CreatedBy).HasColumnName("CREATED_BY");
                entity.Property(p => p.CreationDate).HasColumnName("CREATION_DATE");
                entity.Property(p => p.ModifiedBy).HasColumnName("MODIFIED_BY");
                entity.Property(p => p.ModificationDate).HasColumnName("MODIFICATION_DATE");

                entity.HasQueryFilter(p => !p.IsDeleted);
            });

            // employee_positions
            modelBuilder.Entity<EmployeePosition>(entity =>
            {
                entity.ToTable("EMPLOYEE_POSITIONS");
                entity.Property(ep => ep.Id).HasColumnName("ID");
                entity.Property(ep => ep.EmployeeId).HasColumnName("EMPLOYEE_ID");
                entity.Property(ep => ep.PositionId).HasColumnName("POSITION_ID");
                entity.Property(ep => ep.StartDate).HasColumnName("START_DATE");
                entity.Property(ep => ep.EndDate).HasColumnName("END_DATE");
                entity.Property(ep => ep.IsDeleted).HasColumnName("IS_DELETED");
                entity.Property(ep => ep.CreatedBy).HasColumnName("CREATED_BY");
                entity.Property(ep => ep.CreationDate).HasColumnName("CREATION_DATE");
                entity.Property(ep => ep.ModifiedBy).HasColumnName("MODIFIED_BY");
                entity.Property(ep => ep.ModificationDate).HasColumnName("MODIFICATION_DATE");

                entity.HasOne(ep => ep.Employee)
                      .WithMany()
                      .HasForeignKey(ep => ep.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ep => ep.Position)
                      .WithMany()
                      .HasForeignKey(ep => ep.PositionId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(ep => ep.EmployeeId);
                entity.HasQueryFilter(ep => !ep.IsDeleted);
            });

            // employee_documents
            modelBuilder.Entity<EmployeeDocument>(entity =>
            {
                entity.ToTable("EMPLOYEE_DOCUMENTS");
                entity.Property(d => d.Id).HasColumnName("ID");
                entity.Property(d => d.EmployeeId).HasColumnName("EMPLOYEE_ID");
                entity.Property(d => d.DocumentName).HasColumnName("DOCUMENT_NAME");
                entity.Property(d => d.DocumentPath).HasColumnName("DOCUMENT_PATH");
                entity.Property(d => d.IssueDate).HasColumnName("ISSUE_DATE");
                entity.Property(d => d.ExpiryDate).HasColumnName("EXPIRY_DATE");
                entity.Property(d => d.Notes).HasColumnName("NOTES");
                entity.Property(d => d.CreatedBy).HasColumnName("CREATED_BY");
                entity.Property(d => d.CreationDate).HasColumnName("CREATION_DATE");
                entity.Property(d => d.ModifiedBy).HasColumnName("MODIFIED_BY");
                entity.Property(d => d.ModificationDate).HasColumnName("MODIFICATION_DATE");

                entity.HasOne(d => d.Employee)
                      .WithMany()
                      .HasForeignKey(d => d.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(d => d.EmployeeId);
            });
        }
    }
}
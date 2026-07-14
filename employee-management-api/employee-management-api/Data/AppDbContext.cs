using Microsoft.EntityFrameworkCore;
using EmployeeManagementAPI.Models;

namespace EmployeeManagementAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }

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
                entity.Property(u => u.CreatedBy).HasColumnName("CREATED_BY");
                entity.Property(u => u.CreationDate).HasColumnName("CREATION_DATE");
                entity.Property(u => u.ModifiedBy).HasColumnName("MODIFIED_BY");
                entity.Property(u => u.ModificationDate).HasColumnName("MODIFICATION_DATE");

                entity.HasIndex(u => u.Username).IsUnique();
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
                entity.Property(e => e.CreatedBy).HasColumnName("CREATED_BY");
                entity.Property(e => e.CreationDate).HasColumnName("CREATION_DATE");
                entity.Property(e => e.ModifiedBy).HasColumnName("MODIFIED_BY");
                entity.Property(e => e.ModificationDate).HasColumnName("MODIFICATION_DATE");

                // Employee.Username -> Users.Username (FK)
                entity.HasOne<User>()
                      .WithOne()
                      .HasForeignKey<Employee>(e => e.Username)
                      .HasPrincipalKey<User>(u => u.Username)
                      .OnDelete(DeleteBehavior.Restrict);
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
                entity.Property(d => d.CreatedBy).HasColumnName("CREATED_BY");
                entity.Property(d => d.CreationDate).HasColumnName("CREATION_DATE");
                entity.Property(d => d.ModifiedBy).HasColumnName("MODIFIED_BY");
                entity.Property(d => d.ModificationDate).HasColumnName("MODIFICATION_DATE");

                entity.HasIndex(d => d.DepartmentCode).IsUnique();
            });
        }
    }
}
namespace Alohomora.Sql.Database;

internal abstract class DatabaseContextHelper
{
    internal static string Collation => "Latin1_General_100_CI_AI_SC_UTF8";

    internal static bool EnableDetailedErrors => ApplicationSetting.IsDebugMode;
    internal static bool EnableSensitiveDataLogging => ApplicationSetting.IsDebugMode;

    internal static void EnableIsDeletedQueryFilter(ModelBuilder modelBuilder)
    {
        //ignored
    }

    internal static string ConnectionString
    {
        get
        {
            //m.hafezi
            if (string.Equals(Environment.MachineName, "DESKTOP-M48MB88", StringComparison.CurrentCultureIgnoreCase))
                return "Data Source=.;Initial Catalog=AlohomoraDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";


            //m.hamzeh
            if (string.Equals(Environment.MachineName, "MEYTOL_PC", StringComparison.CurrentCultureIgnoreCase))
                return "Data Source=.;Initial Catalog=VakilAuthDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";


            if (ApplicationSetting.IsDebugMode)
                return "";
            else
                return "";
        }

    }

    internal static void SeedValues(ModelBuilder modelBuilder)
    {
        FillBaseValues_UserStatus(modelBuilder);
    }

    internal static void ConfigureEntities(ModelBuilder modelBuilder)
    {
        ConfigureEntity_AuthOtp(modelBuilder);
        ConfigureEntity_RefreshToken(modelBuilder);
        ConfigureEntity_Role(modelBuilder);
        ConfigureEntity_User(modelBuilder);
        ConfigureEntity_UserRole(modelBuilder);
        ConfigureEntity_UserStatus(modelBuilder);
    }


    #region FillBaseValues

    private static void FillBaseValues_UserStatus(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserStatus>().HasData(new List<UserStatus>()
        {
            new() {
                PublicId = Guid.Parse("00000000-0000-0000-0000-000000000000"),
                Id = UserStatusEnm.Invalid,
                Title = "نا معتبر",
                TitleEn = UserStatusEnm.Invalid.ToString(),
                Description = UserStatusEnm.Invalid.GetDescription()
            },
            new() {
                PublicId = Guid.Parse("1A00F5B7-0422-478D-A45F-CBA8AD6154DF"),
                Id = UserStatusEnm.Active,
                Title = "فعال",
                TitleEn = UserStatusEnm.Active.ToString(),
                Description = UserStatusEnm.Active.GetDescription()
            },
            new() {
                PublicId = Guid.Parse("BE6DAE57-0198-4C31-A492-2EFC5C017524"),
                Id = UserStatusEnm.Deactive,
                Title = "غیر فعال",
                TitleEn = UserStatusEnm.Deactive.ToString(),
                Description = UserStatusEnm.Deactive.GetDescription()
            },
            new() {
                PublicId = Guid.Parse("DEEA2A48-4C8B-4026-8454-158BC2F0C261"),
                Id = UserStatusEnm.Deleted,
                Title = "حذف شده",
                TitleEn = UserStatusEnm.Deleted.ToString(),
                Description = UserStatusEnm.Deleted.GetDescription()
            }
        });

    }


    [Obsolete(message: "", error: true)]
    private static void FillBaseValues_Template(ModelBuilder modelBuilder)
    {
        //modelBuilder.Entity<InvoicePaymentCalculationType>().HasData(
        //   new InvoicePaymentCalculationType()
        //   {
        //       Id = InvoicePaymentCalculationTypeEnm.Unknown,
        //       PublicId = Guid.Parse("00000000-0000-0000-0000-000000000000"),
        //       Title = InvoicePaymentCalculationTypeEnm.Unknown.GetDescription(),
        //       TitleEn = nameof(InvoicePaymentCalculationTypeEnm.Unknown),
        //       Description = string.Empty
        //   },
        //   );
    }

    #endregion

    #region ConfigureEntities

    private static void ConfigureEntity_AuthOtp(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuthOtp>().ToTable(name: nameof(DatabaseContext.AuthOtps), e => e.HasComment("کد های یکبار مصرف"));
        modelBuilder.Entity<AuthOtp>().HasKey(e => e.Id);

        #region Properties

        modelBuilder.Entity<AuthOtp>().Property(p => p.Code)
            .HasMaxLength(6)
            .IsUnicode(false)
            .HasComment("کد یک بار مصرف")
            .IsRequired();

        modelBuilder.Entity<AuthOtp>().Property(p => p.UserPhoneNumber)
            .HasMaxLength(11)
            .IsUnicode(false)
            .IsFixedLength()
            .HasComment("شماره همراه کاربر")
            .IsRequired(false);

        modelBuilder.Entity<AuthOtp>().Property(p => p.UserEmail)
            .HasMaxLength(64)
            .IsUnicode(false)
            .HasComment("ایمیل کاربر")
            .IsRequired(false);

        modelBuilder.Entity<AuthOtp>().Property(p => p.Expires)
            .HasComment("زمان انقضاء کد")
            .IsRequired();

        modelBuilder.Entity<AuthOtp>().Property(p => p.IsUsed)
            .HasComment("آیا این کد استفاده شده است")
            .HasDefaultValue(value: false)
            .IsRequired();

        #endregion

        #region Relations

        modelBuilder.Entity<AuthOtp>()
            .HasOne(e => e.User)
            .WithMany(e => e.AuthOtps)
            .HasForeignKey(e => e.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

        #endregion

        #region Index

        modelBuilder.Entity<AuthOtp>().HasIndex(e => e.PublicId).IsUnique();
        modelBuilder.Entity<AuthOtp>().HasIndex(e => new { e.Code, e.IsUsed, e.UserEmail });
        modelBuilder.Entity<AuthOtp>().HasIndex(e => new { e.Code, e.IsUsed, e.UserPhoneNumber });

        #endregion

    }

    private static void ConfigureEntity_RefreshToken(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>().ToTable(name: nameof(DatabaseContext.RefreshTokens), e => e.HasComment("رفرش توکن ها"));
        modelBuilder.Entity<RefreshToken>().HasKey(e => e.Id);

        #region Properties

        modelBuilder.Entity<RefreshToken>().Property(p => p.Token)
            .HasMaxLength(1024)
            .IsUnicode(false)
            .HasComment("توکن رفرش");

        modelBuilder.Entity<RefreshToken>().Property(p => p.Expires)
            .HasComment("زمان انقضاء کد")
            .IsRequired();

        modelBuilder.Entity<RefreshToken>().Property(p => p.IsRevoked)
            .HasComment("آیا این کد حذف شده است")
            .HasDefaultValue(value: false)
            .IsRequired();

        modelBuilder.Entity<RefreshToken>().Property(p => p.RememberMe)
            .HasComment("مرا به خاطر بسپار")
            .HasDefaultValue(value: false)
            .IsRequired();

        modelBuilder.Entity<RefreshToken>().Property(p => p.AccessTokenId)
            .HasComment("توکن شناسایی")
            .IsRequired();

        #endregion

        #region Relations

        modelBuilder.Entity<RefreshToken>()
            .HasOne(e => e.User)
            .WithMany(e => e.RefreshTokens)
            .HasForeignKey(e => e.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        #endregion

        #region Index

        modelBuilder.Entity<RefreshToken>().HasIndex(e => e.PublicId).IsUnique();
        modelBuilder.Entity<RefreshToken>().HasIndex(e => new { e.Token, e.IsRevoked, e.AccessTokenId });

        #endregion

    }

    private static void ConfigureEntity_Role(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().ToTable(name: nameof(DatabaseContext.Roles), e => e.HasComment("نقش ها"));
        modelBuilder.Entity<Role>().HasKey(e => e.Id);

        #region Properties

        modelBuilder.Entity<Role>().Property(p => p.Name)
            .HasMaxLength(64)
            .IsUnicode(false)
            .IsRequired()
            .HasComment("عنوان نقش");

        modelBuilder.Entity<Role>().Property(p => p.FaName)
            .HasMaxLength(64)
            .IsUnicode()
            .IsRequired()
            .HasComment("عنوان فارسی نقش");

        modelBuilder.Entity<Role>().Property(p => p.Description)
            .HasMaxLength(512)
            .IsUnicode()
            .IsRequired()
            .HasComment("توضیح نقش");

        #endregion

        #region Index

        modelBuilder.Entity<Role>().HasIndex(e => e.PublicId).IsUnique();
        modelBuilder.Entity<Role>().HasIndex(e => e.Name);

        #endregion

    }

    private static void ConfigureEntity_User(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable(name: nameof(DatabaseContext.Users), e => e.HasComment("کاربر ها"));
        modelBuilder.Entity<User>().HasKey(e => e.Id);

        #region Properties

        modelBuilder.Entity<User>().Property(p => p.UserName)
            .HasMaxLength(64)
            .IsUnicode(false)
            .IsRequired()
            .HasComment("نام کاربری");

        modelBuilder.Entity<User>().Property(p => p.PhoneNumber)
            .HasMaxLength(11)
            .IsFixedLength()
            .IsUnicode(false)
            .IsRequired(false)
            .HasComment("شماره تلفن همراه");

        modelBuilder.Entity<User>().Property(p => p.PhoneNumberConfirmed)
            .IsRequired()
            .HasDefaultValue(value: false)
            .HasComment("شماره تلفن همراه تأیید شده است");

        modelBuilder.Entity<User>().Property(p => p.Email)
            .HasMaxLength(128)
            .IsUnicode(false)
            .IsRequired(false)
            .HasComment("ایمیل");

        modelBuilder.Entity<User>().Property(p => p.EmailConfirmed)
            .IsRequired()
            .HasDefaultValue(value: false)
            .HasComment("ایمیل تأیید شده است");

        modelBuilder.Entity<User>().Property(p => p.AccessFailedCount)
            .IsRequired()
            .HasDefaultValue(value: 0)
            .HasComment("تعداد ورود ناموفق");

        modelBuilder.Entity<User>().Property(p => p.LockoutEnabled)
            .IsRequired()
            .HasDefaultValue(value: false)
            .HasComment("آیا کاربر از ورود منع شده است");

        modelBuilder.Entity<User>().Property(p => p.LockoutEnd)
            .IsRequired(false)
            .HasComment("زمان اتمام منع ورود کاربر");

        modelBuilder.Entity<User>().Property(p => p.CanUsePassword)
            .IsRequired()
            .HasDefaultValue(value: false)
            .HasComment("امکان استفاده از رمز عبور");

        modelBuilder.Entity<User>().Property(p => p.PasswordHash)
            .HasMaxLength(256)
            .IsUnicode(false)
            .IsRequired(false)
            .HasComment("رمز عبور هش شده");

        modelBuilder.Entity<User>().Property(p => p.CreatedOn)
            .HasComment("زمان ایجاد کاربر");

        #endregion

        #region Index

        modelBuilder.Entity<User>().HasIndex(e => e.PublicId).IsUnique();
        modelBuilder.Entity<User>().HasIndex(e => e.UserName).IsUnique();
        modelBuilder.Entity<User>().HasIndex(e => e.PhoneNumber);

        #endregion

        #region Relations

        modelBuilder.Entity<User>()
            .HasOne(e => e.UserStatus)
            .WithMany(e => e.Users)
            .HasForeignKey(e => e.UserStatusId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        #endregion
    }

    private static void ConfigureEntity_UserRole(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRole>().ToTable(name: nameof(DatabaseContext.UserRoles), e => e.HasComment("نقش کاربر ها"));
        modelBuilder.Entity<UserRole>().HasKey(e => e.Id);


        #region Index

        modelBuilder.Entity<UserRole>().HasIndex(e => e.PublicId).IsUnique();
        modelBuilder.Entity<UserRole>().HasIndex(e => e.RoleId);
        modelBuilder.Entity<UserRole>().HasIndex(e => e.UserId);
        modelBuilder.Entity<UserRole>().HasIndex(e => new { e.RoleId, e.UserId }).IsUnique();

        #endregion

        #region Relations

        modelBuilder.Entity<UserRole>()
            .HasOne(e => e.User)
            .WithMany(e => e.UserRoles)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<UserRole>()
            .HasOne(e => e.Role)
            .WithMany(e => e.UserRoles)
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.NoAction);

        #endregion
    }

    private static void ConfigureEntity_UserStatus(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserStatus>().ToTable(name: nameof(DatabaseContext.UserStatuses), e => e.HasComment("وضعیت کاربر ها"));
        modelBuilder.Entity<UserStatus>().HasKey(e => e.Id);

        #region Properties

        modelBuilder.Entity<UserStatus>().Property(p => p.TitleEn)
            .HasMaxLength(64)
            .IsUnicode(false)
            .IsRequired()
            .HasComment("عنوان وضعیت");

        modelBuilder.Entity<UserStatus>().Property(p => p.Title)
            .HasMaxLength(64)
            .IsUnicode()
            .IsRequired()
            .HasComment("عنوان فارسی وضعیت");

        modelBuilder.Entity<UserStatus>().Property(p => p.Description)
            .HasMaxLength(512)
            .IsUnicode()
            .IsRequired()
            .HasComment("توضیح وضعیت");

        #endregion

        #region Index

        modelBuilder.Entity<UserStatus>().HasIndex(e => e.PublicId).IsUnique();
        modelBuilder.Entity<UserStatus>().HasIndex(e => e.TitleEn).IsUnique();

        #endregion

    }



    [Obsolete(message: "", error: true)]
    private static void ConfigureEntity_Template(ModelBuilder modelBuilder)
    {
        #region Properties

        //modelBuilder.Entity<Apartment>().Property(p => p.PostalCode)
        //    .HasMaxLength(10)
        //    .IsUnicode(false)
        //    .IsFixedLength()
        //    .HasComment("کد پستی");

        #endregion

        #region Relations

        //modelBuilder.Entity<Apartment>()
        //   .HasMany(e => e.ApartmentPeople)
        //   .WithOne(e => e.Apartment)
        //   .HasForeignKey(e => e.ApartmentId);

        //modelBuilder.Entity<Apartment>()
        //   .HasOne(e => e.ApartmentPeople)
        //   .WithMany(e => e.Apartment)
        //   .HasForeignKey(e => e.ApartmentId);

        #endregion

        #region Index

        //modelBuilder.Entity<Apartment>().HasIndex(e => e.Guid).IsUnique();

        #endregion
    }

    #endregion

}

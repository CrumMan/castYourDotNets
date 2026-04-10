#nullable disable

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace castYourDotNets.Migrations
{
    [DbContext(typeof(castYourDotNets.Data.VerseVaultDbContext))]
    [Migration("20260407143034_InitialCreate")]
    public partial class InitialCreate : Migration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            modelBuilder.Entity("castYourDotNets.Models.PageClass", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");

                b.Property<string>("Book")
                    .IsRequired()
                    .HasMaxLength(128)
                    .HasColumnType("TEXT");

                b.Property<int>("Chapter").HasColumnType("INTEGER");

                b.Property<DateTimeOffset>("CreatedAtUtc").HasColumnType("TEXT");

                b.Property<string>("Notes")
                    .HasMaxLength(2000)
                    .HasColumnType("TEXT");

                b.Property<DateTimeOffset?>("LastReviewedAtUtc").HasColumnType("TEXT");

                b.Property<bool>("IsMemorized").HasColumnType("INTEGER");

                b.Property<Guid>("UserId").HasColumnType("TEXT");

                b.Property<int?>("VerseEnd").HasColumnType("INTEGER");

                b.Property<int>("VerseStart").HasColumnType("INTEGER");

                b.Property<string>("Source")
                    .IsRequired()
                    .HasMaxLength(64)
                    .HasColumnType("TEXT");

                b.Property<DateTimeOffset?>("MemorizedAtUtc").HasColumnType("TEXT");

                b.Property<string>("Text")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.HasIndex("UserId");

                b.ToTable("PageClasses");
            });

            modelBuilder.Entity("castYourDotNets.Models.Scripture", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");

                b.Property<DateTime>("CreatedAtUtc").HasColumnType("TEXT");

                b.Property<DateTime?>("LastPracticedAtUtc").HasColumnType("TEXT");

                b.Property<DateTime?>("MemorizedAtUtc").HasColumnType("TEXT");

                b.Property<bool>("IsMemorized").HasColumnType("INTEGER");

                b.Property<string>("Reference")
                    .IsRequired()
                    .HasMaxLength(150)
                    .HasColumnType("TEXT");

                b.Property<int>("CurrentStreakDays").HasColumnType("INTEGER");

                b.Property<string>("Text")
                    .IsRequired()
                    .HasMaxLength(2000)
                    .HasColumnType("TEXT");

                b.Property<int>("PracticeCount").HasColumnType("INTEGER");

                b.Property<string>("Topic")
                    .HasMaxLength(120)
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.ToTable("Scriptures");
            });

            modelBuilder.Entity("castYourDotNets.Models.UserAccount", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");

                b.Property<DateTimeOffset>("CreatedAtUtc").HasColumnType("TEXT");

                b.Property<string>("NormalizedUsername")
                    .IsRequired()
                    .HasMaxLength(32)
                    .HasColumnType("TEXT");

                b.Property<string>("PasswordHash")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<string>("Username")
                    .IsRequired()
                    .HasMaxLength(32)
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.HasIndex("NormalizedUsername").IsUnique();

                b.ToTable("UserAccounts");
            });

            modelBuilder.Entity("castYourDotNets.Models.MemorizationEntry", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");

                b.Property<DateTime>("CreatedAtUtc").HasColumnType("TEXT");

                b.Property<bool>("HasMemorizationTable").HasColumnType("INTEGER");

                b.Property<bool>("IsMemorized").HasColumnType("INTEGER");

                b.Property<bool>("IsMemorizedThroughGame").HasColumnType("INTEGER");

                b.Property<string>("GameText")
                    .IsRequired()
                    .HasMaxLength(2000)
                    .HasColumnType("TEXT");

                b.Property<DateTime?>("UpdatedAtUtc").HasColumnType("TEXT");

                b.Property<Guid>("UserId").HasColumnType("TEXT");

                b.Property<Guid?>("ScriptureId").HasColumnType("TEXT");

                b.Property<string>("TableInsights")
                    .HasMaxLength(2000)
                    .HasColumnType("TEXT");

                b.Property<string>("TableKeywords")
                    .HasMaxLength(2000)
                    .HasColumnType("TEXT");

                b.Property<string>("TablePersonalApplication")
                    .HasMaxLength(2000)
                    .HasColumnType("TEXT");

                b.Property<string>("TableThemes")
                    .HasMaxLength(2000)
                    .HasColumnType("TEXT");

                b.Property<string>("TableVerses")
                    .HasMaxLength(4000)
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.HasIndex("ScriptureId");

                b.HasIndex("UserId");

                b.ToTable("MemorizationEntries");
            });

            modelBuilder.Entity("castYourDotNets.Models.UserScripture", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");

                b.Property<int>("CurrentStreakDays").HasColumnType("INTEGER");

                b.Property<DateTime>("CreatedAtUtc").HasColumnType("TEXT");

                b.Property<int?>("DifficultyRating").HasColumnType("INTEGER");

                b.Property<bool>("IsMemorized").HasColumnType("INTEGER");

                b.Property<DateTime?>("LastPracticedAtUtc").HasColumnType("TEXT");

                b.Property<int>("LongestStreakDays").HasColumnType("INTEGER");

                b.Property<string>("PersonalNotes")
                    .HasMaxLength(1000)
                    .HasColumnType("TEXT");

                b.Property<int>("Priority").HasColumnType("INTEGER");

                b.Property<int>("PracticeCount").HasColumnType("INTEGER");

                b.Property<Guid>("ScriptureId").HasColumnType("TEXT");

                b.Property<DateTime?>("MemorizedAtUtc").HasColumnType("TEXT");

                b.Property<int>("FailedAttempts").HasColumnType("INTEGER");

                b.Property<int>("SuccessRate").HasColumnType("INTEGER");

                b.Property<int>("SuccessfulAttempts").HasColumnType("INTEGER");

                b.Property<DateTime>("UpdatedAtUtc").HasColumnType("TEXT");

                b.Property<Guid>("UserId").HasColumnType("TEXT");

                b.Property<string>("Status")
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.HasIndex("ScriptureId");

                b.HasIndex("Status");

                b.HasIndex("UserId");

                b.HasIndex("IsMemorized");

                b.HasIndex("LastPracticedAtUtc");

                b.ToTable("UserScriptures");
            });

            modelBuilder.Entity("castYourDotNets.Models.Verse_Vault", b =>
            {
                b.Property<Guid>("id").HasColumnType("TEXT");

                b.Property<int>("scripture").HasColumnType("INTEGER");

                b.Property<string>("book")
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnType("TEXT");

                b.Property<int>("Chapter").HasColumnType("INTEGER");

                b.Property<int>("VerseInt").HasColumnType("INTEGER");

                b.Property<string>("Verse_Refrence")
                    .IsRequired()
                    .HasMaxLength(150)
                    .HasColumnType("TEXT");

                b.Property<string>("VerseText")
                    .IsRequired()
                    .HasMaxLength(2000)
                    .HasColumnType("TEXT");

                b.HasKey("id");

                b.ToTable("VerseVaults");
            });

            modelBuilder.Entity("castYourDotNets.Models.MemorizationEntry", b =>
            {
                b.HasOne("castYourDotNets.Models.Scripture")
                    .WithMany()
                    .HasForeignKey("ScriptureId")
                    .OnDelete(DeleteBehavior.SetNull);

                b.HasOne("castYourDotNets.Models.UserAccount")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity("castYourDotNets.Models.PageClass", b =>
            {
                b.HasOne("castYourDotNets.Models.UserAccount")
                    .WithMany("PageClasses")
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity("castYourDotNets.Models.UserScripture", b =>
            {
                b.HasOne("castYourDotNets.Models.Scripture")
                    .WithMany()
                    .HasForeignKey("ScriptureId")
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne("castYourDotNets.Models.UserAccount")
                    .WithMany("UserScriptures")
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

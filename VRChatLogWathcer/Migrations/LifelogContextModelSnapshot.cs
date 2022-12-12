﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VRChatLogWathcer.Models;

#nullable disable

namespace VRChatLogWathcer.Migrations
{
    [DbContext(typeof(LifelogContext))]
    partial class LifelogContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.3");

            modelBuilder.Entity("VRChatLogWathcer.Models.JoinLeaveHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsLocal")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Joined")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("Left")
                        .HasColumnType("TEXT");

                    b.Property<int>("LocationHistoryId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PlayerName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("LocationHistoryId");

                    b.ToTable("JoinLeaveHistories");
                });

            modelBuilder.Entity("VRChatLogWathcer.Models.LocationHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("InstanceId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Joined")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("Left")
                        .HasColumnType("TEXT");

                    b.Property<int>("LogFileInfoId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("MasterId")
                        .HasColumnType("TEXT");

                    b.Property<int>("Region")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.Property<string>("WorldId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("WorldName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("LogFileInfoId");

                    b.ToTable("LocationHistories");
                });

            modelBuilder.Entity("VRChatLogWathcer.Models.VRChatLogFileInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("LastRead")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("LogFiles");
                });

            modelBuilder.Entity("VRChatLogWathcer.Models.JoinLeaveHistory", b =>
                {
                    b.HasOne("VRChatLogWathcer.Models.LocationHistory", "LocationHistory")
                        .WithMany("JoinLeaveHistories")
                        .HasForeignKey("LocationHistoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("LocationHistory");
                });

            modelBuilder.Entity("VRChatLogWathcer.Models.LocationHistory", b =>
                {
                    b.HasOne("VRChatLogWathcer.Models.VRChatLogFileInfo", "LogFileInfo")
                        .WithMany()
                        .HasForeignKey("LogFileInfoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("LogFileInfo");
                });

            modelBuilder.Entity("VRChatLogWathcer.Models.LocationHistory", b =>
                {
                    b.Navigation("JoinLeaveHistories");
                });
#pragma warning restore 612, 618
        }
    }
}

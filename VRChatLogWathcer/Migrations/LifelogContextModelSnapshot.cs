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
                    b.Property<string>("PlayerName")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Joined")
                        .HasColumnType("TEXT");

                    b.Property<int>("Id")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsLocal")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("Left")
                        .HasColumnType("TEXT");

                    b.Property<int>("LocaionId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("PlayerName", "Joined")
                        .IsUnique();

                    b.ToTable("JoinLeaveHistories");
                });

            modelBuilder.Entity("VRChatLogWathcer.Models.LocationHistory", b =>
                {
                    b.Property<string>("WorldId")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Joined")
                        .HasColumnType("TEXT");

                    b.Property<int>("Id")
                        .HasColumnType("INTEGER");

                    b.Property<string>("InstanceId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("Left")
                        .HasColumnType("TEXT");

                    b.Property<string>("MasterId")
                        .HasColumnType("TEXT");

                    b.Property<int>("Region")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.Property<string>("WorldName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("LocationHistories");
                });
#pragma warning restore 612, 618
        }
    }
}

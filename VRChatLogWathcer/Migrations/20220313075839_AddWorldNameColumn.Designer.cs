﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VRChatLogWathcer.Data;

#nullable disable

namespace VRChatLogWathcer.Migrations
{
    [DbContext(typeof(LifelogContext))]
    [Migration("20220313075839_AddWorldNameColumn")]
    partial class AddWorldNameColumn
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.3");

            modelBuilder.Entity("VRChatLogWathcer.Models.JoinLeaveHistory", b =>
                {
                    b.Property<string>("PlayerName")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Joined")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsLocal")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("Left")
                        .HasColumnType("TEXT");

                    b.HasKey("PlayerName", "Joined");

                    b.ToTable("JoinLeaveHistories");
                });

            modelBuilder.Entity("VRChatLogWathcer.Models.LocationHistory", b =>
                {
                    b.Property<Guid>("WorldId")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Joined")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("Left")
                        .HasColumnType("TEXT");

                    b.Property<string>("WorldName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("WorldId", "Joined");

                    b.ToTable("LocationHistories");
                });
#pragma warning restore 612, 618
        }
    }
}

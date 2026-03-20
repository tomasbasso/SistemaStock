# Backup Google Drive Folder Design

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add hybrid manual/automatic backups of `stock.db` to a user-selected local folder (synced by Google Drive Desktop), with retention and auto-run every 24h.

**Architecture:** A new `BackupService` API creates a SQLite-safe snapshot via `BackupDatabase` into the target folder, updates `Preferences`, and enforces a 15-file rolling window. The service also checks elapsed time for auto-backup. UI in `Configuracion.razor` lets the user pick the folder, view last backup, and trigger manual runs. `MainLayout` kicks off background auto-backup on startup.

**Tech Stack:** .NET 8 MAUI Blazor Hybrid, SQLite via `Microsoft.Data.Sqlite`, `CommunityToolkit.Maui.Storage` (FolderPicker/FileSaver), `Microsoft.Maui.Storage.Preferences`.

---

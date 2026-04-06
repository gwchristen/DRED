# DRED — Device Record Established Database

A C# WinForms desktop application that provides a front-end for a Microsoft Access (`.accdb`) database. DRED supports multi-user access using a split-database design, where the backend `.accdb` file can be placed on a shared network drive while each user runs their own copy of the application.

---

## Prerequisites

Before building or running DRED you need:

| Prerequisite | Notes |
|---|---|
| **Windows** | WinForms is Windows-only |
| **[.NET 8.0 Runtime (Windows)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)** | Required to run the application |
| **[Microsoft Access Database Engine 2016 Redistributable](https://www.microsoft.com/en-us/download/details.aspx?id=54920)** | Required for `.accdb` connectivity via OleDb. Install the 64-bit version if running 64-bit .NET. |
| **[Visual Studio 2022](https://visualstudio.microsoft.com/)** (or the .NET 8 SDK) | Required to build the project |

---

## Project Structure

```
DRED.sln                     Solution file
DRED/
  DRED.csproj                Project file (net8.0-windows, WinForms)
  Program.cs                 Application entry point
  AppSettings.cs             Reads/writes settings.json in %APPDATA%\DRED
  DatabaseHelper.cs          OleDb database creation, connection, CRUD, export
  ExcelImporter.cs           Imports data from Created Histories.xlsx
  ThemeManager.cs            Dark mode theme applied to all forms/controls
  MainForm.cs/.Designer.cs   Main window with tab control and DataGridViews
  RecordForm.cs/.Designer.cs Add/Edit record dialog
  SettingsForm.cs/.Designer.cs  Database path + auto-refresh configuration
  AdvancedSearchForm.cs      Multi-criteria advanced search dialog
Created Histories.xlsx       Source data spreadsheet for initial import
```

---

## Database Schema

There are **4 data tables** plus supporting tables, all created automatically on first run:

| Table | Excel Sheet |
|---|---|
| `OH_Meters` | OH - Meters |
| `IM_Meters` | I&M - Meters |
| `OH_Transformers` | OH - Transformers |
| `IM_Transformers` | I&M - Transformers |

**Data Columns:** `Id` (PK, auto-increment), `OpCo2`, `Status`, `MFR`, `DevCode`, `BegSer`, `EndSer`, `Qty`, `PODate`, `Vintage`, `PONumber`, `RecvDate`, `UnitCost`, `CID`, `MENumber`, `PurCode`, `Est`, `Comments`

**Audit Columns (auto-managed):** `CreatedBy`, `CreatedDate`, `ModifiedBy`, `ModifiedDate`

**Supporting Tables:** `RecordLocks` (for multi-user record locking)

> **Note:** Existing databases are automatically upgraded: the legacy `Key` column is dropped and new audit columns are added via `ALTER TABLE`.

---

## Build & Run

### Using Visual Studio

1. Open `DRED.sln` in Visual Studio 2022.
2. Restore NuGet packages (automatic on build, or right-click solution → Restore NuGet Packages).
3. Build → Start Debugging (`F5`).

### Using the .NET CLI

```bash
cd DRED
dotnet restore
dotnet build
dotnet run --project DRED/DRED.csproj
```

---

## First-Time Setup

1. On first launch, the **Settings** dialog will open automatically.
2. Click **Browse…** and navigate to a folder where the database file should live.
   - For **single-user** use: any local folder (e.g. `C:\DRED\DRED.accdb`).
   - For **multi-user / network** use: a UNC path on a shared drive (e.g. `\\fileserver\dred\DRED.accdb`).
3. Configure the **Auto-Refresh Interval** (seconds; 0 = disabled).
4. Click **OK**. The application will create the `.accdb` file and all tables automatically.

> **Note:** Every user must install the **Microsoft Access Database Engine** and configure the same network path in Settings.

---

## Configuring the Database Path (Multi-User)

- The database path is saved per-machine in `%APPDATA%\DRED\settings.json`.
- To change the path later, click the **⚙ Settings (Ctrl+,)** button in the toolbar.
- Point all users to the same `.accdb` file on the network share.
- The application uses `Mode=Share Deny None` so multiple users can read/write simultaneously.

---

## Importing Data from the Excel Spreadsheet

1. Click **📥 Import (Ctrl+I)** in the toolbar.
2. Navigate to and select `Created Histories.xlsx` (also included in this repository).
3. The importer reads each of the 4 sheets and inserts rows into the corresponding tables.
4. A summary dialog will report how many records were imported from each sheet.

> **Warning:** Importing is additive — running the import multiple times will create duplicate records.

---

## Features

### Core Features
| Feature | Description |
|---|---|
| **Tab-based browsing** | 4 tabs, one per table, each with a searchable DataGridView |
| **Add / Edit / Delete** | Full CRUD via the toolbar; reusable form for all 4 tables |
| **Double-click to Edit** | Double-click any row to open it in edit mode |
| **Search / Filter** | Real-time filter bar searches across all text columns or a specific column |
| **Export to Excel** | Exports the current tab's data to an `.xlsx` file via ClosedXML |
| **Export All to Excel** | Exports all 4 tables into a single workbook with 4 sheets |
| **Import from Excel** | Migration from `Created Histories.xlsx` |
| **Settings** | Configurable database path and auto-refresh interval |

### UI & UX
| Feature | Description |
|---|---|
| **Dark Mode** | Modern flat dark theme (`#1E1E1E` background, `#007ACC` accent) applied via `ThemeManager` |
| **Currency Formatting** | `UnitCost` displayed as `$#,##0.00` in grids; in-form formatting on enter/leave |
| **Date Formatting** | `PODate` and `RecvDate` displayed as `MM/dd/yyyy` in grids |
| **Status Bar** | Shows record count, database path, and current Windows username |

### Date Input
| Feature | Description |
|---|---|
| **PO Date** | Always-enabled DateTimePicker; check "No PO Date" to clear |
| **Recv Date** | Checkbox — when checked, auto-fills with today's date; no manual picking |

### Smart Data Entry
| Feature | Description |
|---|---|
| **Qty Auto-Calculation** | When `BegSer` and `EndSer` are numeric, auto-calculates `Qty = EndSer − BegSer + 1`; use "Auto" checkbox to enable/disable |

### Search & Filter
| Feature | Description |
|---|---|
| **Column-Specific Filter** | Dropdown selects a single column to filter on (or "All Columns") |
| **Advanced Search** | Multi-criteria dialog with text fields, date ranges (PODate, RecvDate), cost range, and qty range |

### Multi-User & Data Integrity
| Feature | Description |
|---|---|
| **Record Locking** | Prevents two users from editing the same record simultaneously; 30-minute stale lock timeout |
| **Audit Trail** | `CreatedBy`/`CreatedDate` and `ModifiedBy`/`ModifiedDate` are automatically recorded and shown in the edit form |
| **Auto-Refresh** | Configurable timer (10–600 seconds, or 0 to disable) refreshes the current tab's data periodically; pauses while a dialog is open |

---

## Keyboard Shortcuts

| Shortcut | Action |
|---|---|
| `Ctrl+N` | Add new record |
| `Ctrl+E` | Edit selected record |
| `Delete` | Delete selected record (with confirmation) |
| `F5` / `Ctrl+R` | Refresh current tab |
| `Ctrl+F` | Focus the search textbox |
| `Ctrl+Shift+F` | Open Advanced Search |
| `Ctrl+S` | Export current tab to Excel |
| `Ctrl+Shift+S` | Export all tabs to Excel |
| `Ctrl+I` | Import from Excel |
| `Ctrl+,` | Open Settings |
| `Escape` | Clear search filter and advanced criteria |

---

## NuGet Dependencies

| Package | Purpose |
|---|---|
| `ClosedXML` | Excel import and export |
| `System.Data.OleDb` | Microsoft Access database connectivity |

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
  MainForm.cs/.Designer.cs   Main window with tab control and DataGridViews
  RecordForm.cs/.Designer.cs Add/Edit record dialog
  SettingsForm.cs/.Designer.cs  Database path configuration dialog
Created Histories.xlsx       Source data spreadsheet for initial import
```

---

## Database Schema

There are **4 tables**, one per sheet in the Excel source file, all sharing the same column structure:

| Table | Excel Sheet |
|---|---|
| `OH_Meters` | OH - Meters |
| `IM_Meters` | I&M - Meters |
| `OH_Transformers` | OH - Transformers |
| `IM_Transformers` | I&M - Transformers |

**Columns:** `Id` (PK), `Key`, `OpCo2`, `Status`, `MFR`, `DevCode`, `BegSer`, `EndSer`, `Qty`, `PODate`, `Vintage`, `PONumber`, `RecvDate`, `UnitCost`, `CID`, `MENumber`, `PurCode`, `Est`, `Comments`

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
3. Click **OK**. The application will create the `.accdb` file and all tables automatically.

> **Note:** Every user must install the **Microsoft Access Database Engine** and configure the same network path in Settings.

---

## Configuring the Database Path (Multi-User)

- The database path is saved per-machine in `%APPDATA%\DRED\settings.json`.
- To change the path later, click the **⚙ Settings** button in the toolbar.
- Point all users to the same `.accdb` file on the network share.
- The application uses `Mode=Share Deny None` so multiple users can read/write simultaneously.

---

## Importing Data from the Excel Spreadsheet

1. Click **📥 Import from Excel** in the toolbar.
2. Navigate to and select `Created Histories.xlsx` (also included in this repository).
3. The importer will read each of the 4 sheets and insert rows into the corresponding tables.
4. A summary dialog will report how many records were imported from each sheet.

> **Warning:** Importing is additive — running the import multiple times will create duplicate records.

---

## Features

| Feature | Description |
|---|---|
| **Tab-based browsing** | 4 tabs, one per table, each with a searchable DataGridView |
| **Add / Edit / Delete** | Full CRUD via the toolbar buttons; reusable form for all 4 tables |
| **Search / Filter** | Real-time filter bar at the top searches across all text columns |
| **Export to Excel** | Exports the current tab's data to an `.xlsx` file via ClosedXML |
| **Import from Excel** | One-time migration from `Created Histories.xlsx` |
| **Settings** | Configurable database path supporting local or network locations |
| **Multi-user support** | OleDb share-deny-none mode; connections opened and closed per operation |

---

## NuGet Dependencies

| Package | Purpose |
|---|---|
| `ClosedXML` | Excel import and export |
| `System.Data.OleDb` | Microsoft Access database connectivity |

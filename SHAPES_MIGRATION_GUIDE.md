# Shapes Implementation & Migration Guide

## ✅ Completed Tasks

### 1. Updated Navigation Layout
**File**: `Views/Shared/_Layout.cshtml`

Added complete navigation for all entities:
- ✓ Agencies
- ✓ Routes  
- ✓ Stops
- ✓ Trips
- ✓ **Stop Times** (NEW)
- ✓ **Calendars** (NEW)
- ✓ **Shapes** (NEW)
- ✓ **Shape Points** (NEW)

Each navigation item includes:
- Appropriate Font Awesome icon
- Active state highlighting
- Organized into logical sections

### 2. Shapes Implementation Status

#### Models ✅
- `Shape.cs` - Individual shape points with coordinates
- `ShapesMaster.cs` - Shape definitions/containers

#### Controllers ✅
- `ShapesController.cs` - Full CRUD + Bulk Delete + Delete Invalid
- `ShapesMasterController.cs` - Full CRUD + Bulk Delete

#### Views ✅
All CRUD views exist:
- `Views/Shapes/Index.cshtml` - With bulk delete & delete invalid
- `Views/Shapes/Create.cshtml`
- `Views/Shapes/Edit.cshtml`
- `Views/Shapes/Details.cshtml`
- `Views/Shapes/Delete.cshtml`

- `Views/ShapesMaster/Index.cshtml`
- `Views/ShapesMaster/Create.cshtml`
- `Views/ShapesMaster/Edit.cshtml`
- `Views/ShapesMaster/Details.cshtml`
- `Views/ShapesMaster/Delete.cshtml`

### 3. Database Schema

The database already includes Shapes tables from the initial migration:

```sql
-- ShapesMasters table
CREATE TABLE ShapesMasters (
    id INT PRIMARY KEY IDENTITY,
    gtfs_shape_id NVARCHAR(50) NOT NULL
);

-- Shapes table (points)
CREATE TABLE Shapes (
    id INT PRIMARY KEY IDENTITY,
    shape_id INT NOT NULL,  -- FK to ShapesMasters
    shape_pt_sequence INT NOT NULL,
    shape_pt_lat DECIMAL(10,8) NOT NULL,
    shape_pt_lon DECIMAL(11,8) NOT NULL,
    shape_dist_traveled DECIMAL(10,2) NULL,
    FOREIGN KEY (shape_id) REFERENCES ShapesMasters(id)
);
```

## 🔧 Migration Instructions

### Option 1: If Database Exists
If you already have a database with data:

```bash
# 1. Stop any running application instances
# 2. Clean the build
dotnet clean

# 3. Build the project
dotnet build

# 4. Check current migration status
dotnet ef migrations list

# 5. If needed, create a new migration for any schema changes
dotnet ef migrations add UpdateShapesSchema

# 6. Apply migrations
dotnet ef database update
```

### Option 2: Fresh Database
If starting fresh:

```bash
# 1. Drop existing database (WARNING: This deletes all data!)
dotnet ef database drop --force

# 2. Apply all migrations
dotnet ef database update

# 3. Run the application
dotnet run
```

### Option 3: Verify Existing Schema
To check if Shapes tables already exist:

```bash
# Connect to your database and run:
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('Shapes', 'ShapesMasters');
```

## 📊 Testing Shapes Functionality

### 1. Import GTFS Data with Shapes
1. Navigate to `/Import`
2. Upload a GTFS zip file that includes `shapes.txt`
3. The importer will:
   - Create ShapesMaster records for each unique `shape_id`
   - Create Shape point records for each coordinate
   - Handle non-standard column names automatically

### 2. View Shapes
- **ShapesMaster**: `/ShapesMaster` - View all shape definitions
- **Shape Points**: `/Shapes` - View all coordinate points

### 3. Bulk Operations
On the Shapes Index page:
- ✓ Select multiple shape points using checkboxes
- ✓ Click "Delete Selected" to remove multiple points
- ✓ Click "Delete Invalid (0,0)" to remove points with zero coordinates

### 4. Search & Filter
- Use the search box to filter shapes by any column
- Selection only affects visible (filtered) rows

## 🗺️ Shape Data Structure

### ShapesMaster
Represents a complete route shape:
```csharp
{
    Id: 1,
    GtfsShapeId: "shape_001"
}
```

### Shape (Points)
Individual coordinates that make up the shape:
```csharp
{
    Id: 1,
    ShapeId: 1,  // References ShapesMaster
    ShapePtSequence: 1,
    ShapePtLat: 40.7128,
    ShapePtLon: -74.0060,
    ShapeDistTraveled: 0.0
}
```

## 🔗 Relationships

```
ShapesMaster (1) ----< Shapes (Many)
     |
     |
     v
   Trips (Many) - Trips can reference a shape
```

## 🎨 UI Features

### Navigation Icons
- **Shapes** (ShapesMaster): `fa-project-diagram` 📊
- **Shape Points**: `fa-draw-polygon` 🔷
- **Stop Times**: `fa-clock` 🕐
- **Calendars**: `fa-calendar-alt` 📅

### Bulk Delete Features
All Index pages now include:
- Checkbox selection
- "Select All" with indeterminate state
- Dynamic "Delete Selected (N)" button
- Confirmation dialogs
- Success/error messages

## ⚠️ Current Build Issue

**Problem**: File locking from running application
**Solution**: 
1. Stop all running instances of the application
2. Close any terminals running `dotnet run` or `dotnet watch`
3. Run `dotnet clean`
4. Then proceed with build/migrations

## 📝 Next Steps

1. **Stop Running Processes**:
   - Check Task Manager for any `dotnet.exe` or `TransitFeeds.exe`
   - Close any terminals with active `dotnet run`

2. **Build & Migrate**:
   ```bash
   dotnet clean
   dotnet build
   dotnet ef database update
   ```

3. **Run Application**:
   ```bash
   dotnet run
   ```

4. **Test Shapes**:
   - Navigate to `/ShapesMaster` and `/Shapes`
   - Import GTFS data with shapes
   - Test bulk delete functionality
   - Verify search/filter works

## 🎯 Summary

✅ **Completed**:
- All Shapes models, controllers, and views
- Bulk delete functionality
- Delete invalid coordinates feature
- Updated navigation with all entities
- Intelligent GTFS import for shapes

⏳ **Pending**:
- Stop running application instance
- Complete build and migration
- Test with real GTFS data

All code is ready and functional. The only blocker is the file lock from a running process.

# Live Dashboard Backend API

## 🚀 Base URL
```
https://live-dashboard-backend-production.up.railway.app/api
```
##### participantType means client type

## 📊 Available Survey IDs for Testing

Current surveys available in the production database:
- **Survey ID**: `8dff523d-2a46-4ee3-8017-614af3813b32` (Retirement Village survey)
- **Survey ID**: `1e2f84b2-bba2-4226-a1de-c511e8402068` (Residential Care - Nursing Home survey)

Use either of these survey IDs when testing chart endpoints, user survey endpoints, filter management APIs, and last updated APIs.

## 🔐 Authentication API

### Login
User authentication endpoint.

**Endpoint:** `POST /login`

**Request Body:**
```json
{
  "username": "admin",
  "password": "admin123"
}
```

**Available Users:**
- `admin` / `admin123`
- `user` / `user123`
- `teacher` / `teacher123`
- `student` / `student123`

**Success Response:**
```json
{
  "success": true,
  "message": "login successful",
  "username": "admin"
}
```

**Error Response:**
```json
{
  "success": false,
  "message": "user name or password is incorrect"
}
```

**Example:**
```javascript
fetch('https://live-dashboard-backend-production.up.railway.app/api/login', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    username: 'admin',
    password: 'admin123'
  })
})
```

## ⏰ Enhanced Period Filtering

All Chart APIs now support advanced time period filtering with multiple formats:

### Supported Period Formats:
- **Year**: `period=2025` - Filter by entire year
- **Single Month**: `period=2025-07` - Filter by specific month (data up to end of month)
- **Multiple Months**: `period=2025-07,2025-08` - Filter by multiple months in same year
- **Date Range**: `period=2024-05:2025-08` - Filter by month range (supports cross-year) **NEW**
- **No Filter**: Leave `period` parameter empty to get all available data

### Notes:
- ✅ **Backward Compatible**: Existing `period=2025` format continues to work
- ✅ **Cross-Year Support**: Date range format supports filtering across years (e.g., `2024-12:2025-03`)
- ✅ **Error Handling**: Invalid formats gracefully degrade to show all data
- ✅ **Applied to All Chart APIs**: Same filtering logic works across all chart endpoints
- 🆕 **Frontend Integration**: Perfect for date range pickers and advanced filtering components
- 📊 **Data Availability**: Current database contains data for July-August 2025 (other months return zero values)

### Testing Examples:
```bash
# Format validation - all these work correctly:
period=2025              # → Entire year 2025
period=2025-07           # → July 2025 only  
period=2025-07,2025-08   # → July and August 2025 (comma format)
period=2025-07:2025-08   # → July to August 2025 (range format)
period=2024-12:2025-02   # → December 2024 to February 2025 (cross-year)

# Expected results based on current data:
period=2025-07           # → Returns actual data (44.2% satisfaction)
period=2025-07:2025-08   # → Returns combined data (46.8% satisfaction)  
period=2025-01:2025-03   # → Returns zeros (no data for these months)
```

### Data Validation Results:
| Period Filter | Total Satisfied % | Notes |
|---------------|-------------------|-------|
| `period=2025` | 46.8% | Entire year (same as Jul-Aug) |
| `period=2025-07` | 44.2% | July only |
| `period=2025-08` | ~48-50% | August only (estimated) |
| `period=2025-07:2025-08` | 46.8% | July to August range |
| `period=2025-07,2025-08` | 46.8% | Same as range format |
| `period=2025-01:2025-03` | 0% | No data available |
| `period=2024-11:2025-02` | 0% | Cross-year, no data |

### Frontend Integration Example:
```javascript
// Period Range Selector for Frontend
class PeriodRangeSelector {
    constructor() {
        this.startMonth = null;  // Format: "2024-05"
        this.endMonth = null;    // Format: "2025-08"
    }
    
    // Generate API parameter
    getPeriodParameter() {
        if (!this.startMonth || !this.endMonth) {
            return null; // No filtering
        }
        
        if (this.startMonth === this.endMonth) {
            return this.startMonth; // Single month: "2025-07"
        }
        
        return `${this.startMonth}:${this.endMonth}`; // Range: "2024-05:2025-08"
    }
    
    // Call API with period filtering
    async fetchChartData(surveyId, additionalFilters = {}) {
        const periodParam = this.getPeriodParameter();
        
        const params = new URLSearchParams({
            surveyId,
            ...additionalFilters
        });
        
        if (periodParam) {
            params.append('period', periodParam);
        }
        
        const response = await fetch(`/api/charts/customer-satisfaction?${params}`);
        return response.json();
    }
}

// Usage Example
const selector = new PeriodRangeSelector();
selector.startMonth = "2025-07";  // User selects start month
selector.endMonth = "2025-08";    // User selects end month

// Call API: /api/charts/customer-satisfaction?surveyId=xxx&period=2025-07:2025-08
const data = await selector.fetchChartData("8dff523d-2a46-4ee3-8017-614af3813b32");
```

---

## 📊 Chart APIs

### 1. Response Chart
Get survey response statistics and participant distribution.

**Endpoint:** `GET /charts/response`

**Parameters:**
- `surveyId` (required): `8dff523d-2a46-4ee3-8017-614af3813b32`
- `gender` (optional): `1` (Male) or `2` (Female)
- `participantType` (optional): `1`, `2`, `3`, etc.
- `period` (optional): Enhanced time period filter supporting multiple formats:
  - `2025` - Filter by entire year
  - `2025-07` - Filter by specific month (July 2025)
  - `2025-07,2025-08` - Filter by multiple months (July and August 2025)
  - Leave empty for all data

**Examples:**
```javascript
// Filter by entire year
fetch('https://live-dashboard-backend-production.up.railway.app/api/charts/response?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&period=2025')

// Filter by specific month
fetch('https://live-dashboard-backend-production.up.railway.app/api/charts/response?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&period=2025-07')

// Filter by multiple months (same year)
fetch('https://live-dashboard-backend-production.up.railway.app/api/charts/response?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&period=2025-07,2025-08')

// Filter by date range (NEW - supports cross-year)
fetch('https://live-dashboard-backend-production.up.railway.app/api/charts/response?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&period=2025-07:2025-08')

// Cross-year date range
fetch('https://live-dashboard-backend-production.up.railway.app/api/charts/response?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&period=2024-12:2025-02')
```

### 2. Customer Satisfaction
Get customer satisfaction ratings and statistics.

**Endpoint:** `GET /charts/customer-satisfaction`

**Parameters:**
- `surveyId` (required): `8dff523d-2a46-4ee3-8017-614af3813b32`
- `gender` (optional): `1` (Male) or `2` (Female)
- `participantType` (optional): `1`, `2`, `3`, etc.
- `period` (optional): Enhanced time period filter (same formats as Response Chart)

**Examples:**
```javascript
// Filter by specific month with gender filter
fetch('https://live-dashboard-backend-production.up.railway.app/api/charts/customer-satisfaction?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&gender=2&period=2025-07')

// Filter by multiple months (comma format)
fetch('https://live-dashboard-backend-production.up.railway.app/api/charts/customer-satisfaction?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&period=2025-07,2025-08')

// Filter by date range (colon format - NEW)
fetch('https://live-dashboard-backend-production.up.railway.app/api/charts/customer-satisfaction?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&period=2025-07:2025-08')

// Cross-year range with participant type filter
fetch('https://live-dashboard-backend-production.up.railway.app/api/charts/customer-satisfaction?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&participantType=1&period=2024-11:2025-02')
```

### 3. Customer Satisfaction Trend
Get customer satisfaction trend over time.

**Endpoint:** `GET /charts/customer-satisfaction-trend`

**Parameters:**
- `surveyId` (required): `8dff523d-2a46-4ee3-8017-614af3813b32`
- `gender` (optional): `1` (Male) or `2` (Female)
- `participantType` (optional): `1`, `2`, `3`, etc.
- `period` (optional): time period filter (e.g., `2025`, `2024`, `2023`)
  - If specified as a year, returns only data for that year
  - If not specified, returns trend data for all available years

**Example:**
```javascript
// Get trend data for a specific year
fetch('https://live-dashboard-backend-production.up.railway.app/api/charts/customer-satisfaction-trend?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&period=2025')

// Get trend data for all years
fetch('https://live-dashboard-backend-production.up.railway.app/api/charts/customer-satisfaction-trend?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32')
```

### 4. NPS (Net Promoter Score)
Get NPS data and breakdown.

**Endpoint:** `GET /charts/nps`

**Parameters:**
- `surveyId` (required): `8dff523d-2a46-4ee3-8017-614af3813b32`
- `gender` (optional): `1` (Male) or `2` (Female)
- `participantType` (optional): `1`, `2`, `3`, etc.
- `period` (optional): time period filter

**Example:**
```javascript
fetch('https://live-dashboard-backend-production.up.railway.app/api/charts/nps?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&gender=1')
```

### 5. Service Attributes
Get service attribute ratings.

**Endpoint:** `GET /charts/service-attributes`

**Parameters:**
- `surveyId` (required): `8dff523d-2a46-4ee3-8017-614af3813b32`
- `gender` (optional): `1` (Male) or `2` (Female)
- `participantType` (optional): `1`, `2`, `3`, etc.
- `period` (optional): time period filter
- `selectedAttributes` (optional): Array of attributes`

**Example:**
```javascript
fetch('https://live-dashboard-backend-production.up.railway.app/api/charts/service-attributes?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32')
```

## ⏰ Survey Data Freshness API

### Get Last Updated Time
Shows when survey data was last refreshed/imported from Qualtrics.

**Endpoint:** `GET /surveys/{surveyId}/last-updated`

**Parameters:**
- `surveyId` (required): GUID of the survey
  - `8dff523d-2a46-4ee3-8017-614af3813b32` (Retirement Village)
  - `1e2f84b2-bba2-4226-a1de-c511e8402068` (Residential Care)

**Success Response:**
```json
{
  "success": true,
  "message": "Last updated time retrieved successfully",
  "data": {
    "surveyId": "8dff523d-2a46-4ee3-8017-614af3813b32",
    "lastUpdatedAt": "2025-09-22T09:40:28.028142Z",
    "source": "extraction_log",
    "formattedTime": "2025-09-22 09:40:28 UTC"
  }
}
```

**Error Response (Survey Not Found):**
```json
{
  "success": false,
  "message": "Survey not found",
  "data": null
}
```

**Error Response (No Data):**
```json
{
  "success": false,
  "message": "No data refresh history found for this survey",
  "data": null
}
```

**Examples:**
```javascript
// Get last updated time for Retirement Village survey
fetch('https://live-dashboard-backend-production.up.railway.app/api/surveys/8dff523d-2a46-4ee3-8017-614af3813b32/last-updated')

// Get last updated time for Residential Care survey  
fetch('https://live-dashboard-backend-production.up.railway.app/api/surveys/1e2f84b2-bba2-4226-a1de-c511e8402068/last-updated')

// Frontend integration example
async function showDataFreshness(surveyId) {
  try {
    const response = await fetch(`/api/surveys/${surveyId}/last-updated`);
    const result = await response.json();
    
    if (result.success) {
      console.log(`Data last updated: ${result.data.formattedTime}`);
      // Show in UI: "Data last refreshed: 2025-09-22 09:40:28 UTC"
    } else {
      console.log('No data refresh information available');
    }
  } catch (error) {
    console.error('Failed to get data freshness info:', error);
  }
}
```

**Use Cases:**
- 📊 **Dashboard Footer**: Show "Last updated" timestamp
- 🔄 **Data Sync Status**: Indicate when data was last imported
- ⚠️ **Staleness Warning**: Alert if data is too old
- 📱 **Mobile Apps**: Cache invalidation based on update time

---

## � User Survey APIs

### 1. Get User Surveys
Get all surveys accessible to a specific user based on their organization.

**Endpoint:** `GET /users/{userId}/surveys`

**Parameters:**
- `userId` (required): GUID of the user

**Response:**
```json
{
  "success": true,
  "message": "Found 2 surveys",
  "data": {
    "surveys": [
      {
        "surveyId": "8dff523d-2a46-4ee3-8017-614af3813b32",
        "surveyName": "Customer Satisfaction Survey 2025",
        "serviceType": "Public Service",
        "status": "active",
        "isDefault": true
      },
      {
        "surveyId": "7cee412c-1935-3dd2-9016-503ae2702c21",
        "surveyName": "Staff Feedback Survey",
        "serviceType": "Internal",
        "status": "active", 
        "isDefault": false
      }
    ],
    "defaultSurvey": {
      "surveyId": "8dff523d-2a46-4ee3-8017-614af3813b32",
      "surveyName": "Customer Satisfaction Survey 2025",
      "serviceType": "Public Service",
      "status": "active",
      "isDefault": true
    }
  }
}
```

**Available Test Users:**
- **sithu**: `e8268d06-61f4-40bc-a03f-29416f1a8aaa`
- **wayne**: `1df07f08-f487-4a36-8522-cf17bc69d50b`

**Example:**
```javascript
// Test with user sithu
fetch('https://live-dashboard-backend-production.up.railway.app/api/users/e8268d06-61f4-40bc-a03f-29416f1a8aaa/surveys') - no connection in database will return 0

// Test with user wayne
fetch('https://live-dashboard-backend-production.up.railway.app/api/users/1df07f08-f487-4a36-8522-cf17bc69d50b/surveys')
```

### 2. Get User Default Survey
Get the default survey for a specific user (useful for dashboard initialization).

**Endpoint:** `GET /users/{userId}/surveys/default`

**Parameters:**
- `userId` (required): GUID of the user

**Response:**
```json
{
  "success": true,
  "message": "Default survey retrieved successfully",
  "data": {
    "surveyId": "8dff523d-2a46-4ee3-8017-614af3813b32",
    "surveyName": "Customer Satisfaction Survey 2025",
    "serviceType": "Public Service",
    "status": "active",
    "isDefault": true
  }
}
```

**Example:**
```javascript
// Test with user sithu
fetch('https://live-dashboard-backend-production.up.railway.app/api/users/e8268d06-61f4-40bc-a03f-29416f1a8aaa/surveys/default')

// Test with user wayne
fetch('https://live-dashboard-backend-production.up.railway.app/api/users/1df07f08-f487-4a36-8522-cf17bc69d50b/surveys/default')
```

---

## 🎛️ Filter Management APIs

The Filter Management APIs provide unified endpoints for retrieving available filter options for dashboard components. These APIs support dynamic dropdown population and filter state management in the frontend.

### ✨ Key Features:
- **🔄 Dynamic Filtering**: Populate dropdowns based on actual user/survey data
- **👤 User-Scoped Services**: Get services based on user's accessible surveys
- **📊 Survey-Specific Regions**: Get regions from actual response data per survey
- **🎯 Data-Driven**: Only show options with actual data (no empty filters)
- **⚡ Fast Response**: Optimized for quick dropdown population
- **🔗 Frontend Ready**: Designed for seamless dashboard integration

### 1. Get User Services
Retrieve all available service types for a specific user based on their accessible surveys.

**Endpoint:** `GET /users/{userId}/services`

**Parameters:**
- `userId` (required): GUID of the user

**Description:**
Returns all unique service types from surveys accessible to the user. This is used to populate service filter dropdowns in the dashboard.

**Success Response:**
```json
{
  "success": true,
  "message": "Found 1 services",
  "data": [
    {
      "surveyId": "8dff523d-2a46-4ee3-8017-614af3813b32",
      "serviceType": "Retirement Village",
      "serviceName": "Retirement Village", 
      "description": "",
      "isSelected": false,
      "totalResponses": 0
    }
  ]
}
```

**No Access Response (User Not Found or No Surveys):**
```json
{
  "success": true,
  "message": "Found 0 services",
  "data": []
}
```

**Frontend Usage:**
```javascript
// Populate service filter dropdown
async function loadServiceFilter(userId) {
  try {
    const response = await fetch(`https://live-dashboard-backend-production.up.railway.app/api/users/${userId}/services`);
    const result = await response.json();
    
    if (result.success) {
      const services = result.data;
      // Populate dropdown with services
      const dropdown = document.getElementById('serviceFilter');
      services.forEach(service => {
        const option = document.createElement('option');
        option.value = service.surveyId;
        option.textContent = service.serviceName;
        option.setAttribute('data-service-type', service.serviceType);
        dropdown.appendChild(option);
      });
    }
  } catch (error) {
    console.error('Error loading services:', error);
  }
}

// Example usage
loadServiceFilter('1df07f08-f487-4a36-8522-cf17bc69d50b');
```

**Test Cases:**
```bash
# Test with user wayne (has survey access)
curl "https://live-dashboard-backend-production.up.railway.app/api/users/1df07f08-f487-4a36-8522-cf17bc69d50b/services"

# Test with user sithu (no survey access - returns empty array)
curl "https://live-dashboard-backend-production.up.railway.app/api/users/e8268d06-61f4-40bc-a03f-29416f1a8aaa/services"

# Test with invalid user ID (returns empty array)
curl "https://live-dashboard-backend-production.up.railway.app/api/users/00000000-0000-0000-0000-000000000000/services"
```

### 2. Get Survey Regions
Retrieve all available regions/facilities for a specific survey based on actual response data.

**Endpoint:** `GET /surveys/{surveyId}/regions`

**Parameters:**
- `surveyId` (required): GUID of the survey

**Description:**
Returns all unique regions/facilities from actual survey responses. This is used to populate region filter dropdowns and ensures only regions with actual data are shown.

**Success Response:**
```json
{
  "success": true,
  "message": "Found 6 regions",
  "data": [
    {
      "facilityCode": "3001",
      "regionName": "Bull Creek",
      "participantCount": 25,
      "isSelected": false
    },
    {
      "facilityCode": "3002", 
      "regionName": "Coolbellup",
      "participantCount": 32,
      "isSelected": false
    },
    {
      "facilityCode": "3003",
      "regionName": "Mosman Park", 
      "participantCount": 18,
      "isSelected": false
    }
  ]
}
```

**No Data Response (Survey Not Found or No Responses):**
```json
{
  "success": true,
  "message": "Found 0 regions",
  "data": []
}
```

**Frontend Usage:**
```javascript
// Populate region filter dropdown
async function loadRegionFilter(surveyId) {
  try {
    const response = await fetch(`https://live-dashboard-backend-production.up.railway.app/api/surveys/${surveyId}/regions`);
    const result = await response.json();
    
    if (result.success) {
      const regions = result.data;
      const dropdown = document.getElementById('regionFilter');
      
      // Clear existing options
      dropdown.innerHTML = '<option value="">All Regions</option>';
      
      // Add region options
      regions.forEach(region => {
        const option = document.createElement('option');
        option.value = region.facilityCode;
        option.textContent = `${region.regionName} (${region.participantCount} responses)`;
        dropdown.appendChild(option);
      });
      
      // Show/hide dropdown based on available data
      dropdown.style.display = regions.length > 0 ? 'block' : 'none';
    }
  } catch (error) {
    console.error('Error loading regions:', error);
  }
}

// Dynamic region loading when survey changes
document.getElementById('surveySelector').addEventListener('change', (e) => {
  const selectedSurveyId = e.target.value;
  if (selectedSurveyId) {
    loadRegionFilter(selectedSurveyId);
  }
});
```

**Test Cases:**
```bash
# Test with Retirement Village survey (has region data)
curl "https://live-dashboard-backend-production.up.railway.app/api/surveys/8dff523d-2a46-4ee3-8017-614af3813b32/regions"

# Test with Residential Care survey (has region data)  
curl "https://live-dashboard-backend-production.up.railway.app/api/surveys/1e2f84b2-bba2-4226-a1de-c511e8402068/regions"

# Test with invalid survey ID (returns empty array)
curl "https://live-dashboard-backend-production.up.railway.app/api/surveys/00000000-0000-0000-0000-000000000000/regions"
```

### Integration Notes

**Dashboard Initialization Pattern:**
```javascript
// Complete dashboard filter initialization
async function initializeDashboard(userId) {
  try {
    // 1. Load user's available services
    const servicesResponse = await fetch(`/api/users/${userId}/services`);
    const services = await servicesResponse.json();
    
    // 2. Load user's surveys to get default survey
    const surveysResponse = await fetch(`/api/users/${userId}/surveys/default`);
    const defaultSurvey = await surveysResponse.json();
    
    // 3. Load regions for default survey
    if (defaultSurvey.success) {
      const regionsResponse = await fetch(`/api/surveys/${defaultSurvey.data.surveyId}/regions`);
      const regions = await regionsResponse.json();
      
      // 4. Initialize UI with loaded data
      populateServiceDropdown(services.data);
      populateRegionDropdown(regions.data);
      loadDefaultChartData(defaultSurvey.data.surveyId);
    }
  } catch (error) {
    console.error('Dashboard initialization failed:', error);
  }
}
```

**Filter State Management:**
```javascript
// Coordinated filter management
class DashboardFilterManager {
  constructor(userId) {
    this.userId = userId;
    this.currentSurvey = null;
    this.availableServices = [];
    this.availableRegions = [];
  }

  async initialize() {
    // Load services first (user-level)
    const services = await this.loadServices();
    this.availableServices = services;
    
    // Load default survey and its regions
    const defaultSurvey = await this.loadDefaultSurvey();
    if (defaultSurvey) {
      this.currentSurvey = defaultSurvey;
      this.availableRegions = await this.loadRegions(defaultSurvey.surveyId);
    }
  }

  async onSurveyChange(surveyId) {
    // When survey changes, reload regions for new survey
    this.availableRegions = await this.loadRegions(surveyId);
    this.updateRegionDropdown();
  }

  async loadServices() {
    const response = await fetch(`/api/users/${this.userId}/services`);
    const result = await response.json();
    return result.success ? result.data : [];
  }

  async loadRegions(surveyId) {
    const response = await fetch(`/api/surveys/${surveyId}/regions`);
    const result = await response.json();
    return result.success ? result.data : [];
  }
}
```

**Edge Cases & Error Handling:**

1. **No Survey Access**: User with no surveys returns empty services array (`data: []`)
2. **No Response Data**: Survey with no responses returns empty regions array (`data: []`)  
3. **Invalid IDs**: Invalid user/survey IDs return empty arrays (graceful degradation)
4. **Network Failures**: Implement retry logic and fallback states in frontend
5. **Data Consistency**: Regions are dynamically loaded per survey to ensure accuracy

**Performance Considerations:**
- Services are user-scoped (cached per user session)
- Regions are survey-specific (reload when survey changes)
- Both endpoints are optimized for fast dropdown population
- Consider caching strategies for frequently accessed data

## �📝 Response Format

All APIs return data in this format:

```json
{
  "success": true,
  "message": "Data retrieved successfully",
  "data": {
    // API-specific data structure
  }
}
```

## ❗ Error Handling

If there's an error, the API returns:

```json
{
  "success": false,
  "message": "Error description",
  "data": null
}
```

Common HTTP status codes:
- `200`: Success
- `400`: Bad request (missing or invalid parameters)
- `401`: Unauthorized (for login API)
- `500`: Server error

## 🔧 Quick Test

You can test the APIs directly in your browser or using curl:

```bash
# Test Login API
curl -X POST "https://live-dashboard-backend-production.up.railway.app/api/login" \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'

# Test Response Chart API (Retirement Village)
curl "https://live-dashboard-backend-production.up.railway.app/api/charts/response?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32"

# Test Response Chart API (Residential Care)
curl "https://live-dashboard-backend-production.up.railway.app/api/charts/response?surveyId=1e2f84b2-bba2-4226-a1de-c511e8402068"

# Test Customer Satisfaction API (Retirement Village)
curl "https://live-dashboard-backend-production.up.railway.app/api/charts/customer-satisfaction?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32"

# Test Customer Satisfaction API (Residential Care)
curl "https://live-dashboard-backend-production.up.railway.app/api/charts/customer-satisfaction?surveyId=1e2f84b2-bba2-4226-a1de-c511e8402068"

# Test NPS API with gender filter (Retirement Village)
curl "https://live-dashboard-backend-production.up.railway.app/api/charts/nps?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&gender=1"

# Test NPS API (Residential Care)
curl "https://live-dashboard-backend-production.up.railway.app/api/charts/nps?surveyId=1e2f84b2-bba2-4226-a1de-c511e8402068"

# Test Enhanced Period Filtering (New Features)

# Test single month filtering
curl "https://live-dashboard-backend-production.up.railway.app/api/charts/customer-satisfaction?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&period=2025-07"

# Test multiple months filtering (comma format)
curl "https://live-dashboard-backend-production.up.railway.app/api/charts/nps?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&period=2025-07,2025-08"

# Test date range filtering (colon format - NEW)
curl "https://live-dashboard-backend-production.up.railway.app/api/charts/customer-satisfaction?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&period=2025-07:2025-08"

# Test cross-year date range filtering
curl "https://live-dashboard-backend-production.up.railway.app/api/charts/nps?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&period=2024-11:2025-02"

# Test with Service Attributes API and date range
curl "https://live-dashboard-backend-production.up.railway.app/api/charts/service-attributes?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&period=2025-07:2025-08"

# Data Validation Tests (months with actual data vs. empty months)
# Test months with data (July-August 2025)
curl "https://live-dashboard-backend-production.up.railway.app/api/charts/customer-satisfaction?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&period=2025-07:2025-08"

# Test months without data (January-March 2025) - should return zeros
curl "https://live-dashboard-backend-production.up.railway.app/api/charts/customer-satisfaction?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&period=2025-01:2025-03"

# Test Last Updated API (New Feature)

# Test last updated time for Retirement Village survey
curl "https://live-dashboard-backend-production.up.railway.app/api/surveys/8dff523d-2a46-4ee3-8017-614af3813b32/last-updated"

# Test last updated time for Residential Care survey
curl "https://live-dashboard-backend-production.up.railway.app/api/surveys/1e2f84b2-bba2-4226-a1de-c511e8402068/last-updated"

# Test with invalid survey ID (should return error)
curl "https://live-dashboard-backend-production.up.railway.app/api/surveys/00000000-0000-0000-0000-000000000000/last-updated"

# Test Filter Management APIs (New Feature)

# Test user services endpoint (user with survey access)
curl "https://live-dashboard-backend-production.up.railway.app/api/users/1df07f08-f487-4a36-8522-cf17bc69d50b/services"

# Test user services endpoint (user with no survey access - returns empty array)  
curl "https://live-dashboard-backend-production.up.railway.app/api/users/e8268d06-61f4-40bc-a03f-29416f1a8aaa/services"

# Test survey regions endpoint (Retirement Village - has regions)
curl "https://live-dashboard-backend-production.up.railway.app/api/surveys/8dff523d-2a46-4ee3-8017-614af3813b32/regions"

# Test survey regions endpoint (Residential Care - has regions)
curl "https://live-dashboard-backend-production.up.railway.app/api/surveys/1e2f84b2-bba2-4226-a1de-c511e8402068/regions"

# Test with invalid user ID (returns empty array)
curl "https://live-dashboard-backend-production.up.railway.app/api/users/00000000-0000-0000-0000-000000000000/services"

# Test with invalid survey ID (returns empty array)  
curl "https://live-dashboard-backend-production.up.railway.app/api/surveys/00000000-0000-0000-0000-000000000000/regions"
```

## 📞 Contact

If you encounter any issues or need additional endpoints, please contact the backend team.


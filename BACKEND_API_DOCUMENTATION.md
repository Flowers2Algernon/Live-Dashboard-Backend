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

Use either of these survey IDs when testing chart endpoints and user survey endpoints.

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

## 📊 Chart APIs

### 1. Response Chart
Get survey response statistics and participant distribution.

**Endpoint:** `GET /charts/response`

**Parameters:**
- `surveyId` (required): `8dff523d-2a46-4ee3-8017-614af3813b32`
- `gender` (optional): `1` (Male) or `2` (Female)
- `participantType` (optional): `1`, `2`, `3`, etc.
- `period` (optional): time period filter

**Example:**
```javascript
fetch('https://live-dashboard-backend-production.up.railway.app/api/charts/response?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&gender=1&participantType=2')
```

### 2. Customer Satisfaction
Get customer satisfaction ratings and statistics.

**Endpoint:** `GET /charts/customer-satisfaction`

**Parameters:**
- `surveyId` (required): `8dff523d-2a46-4ee3-8017-614af3813b32`
- `gender` (optional): `1` (Male) or `2` (Female)
- `participantType` (optional): `1`, `2`, `3`, etc.
- `period` (optional): time period filter

**Example:**
```javascript
fetch('https://live-dashboard-backend-production.up.railway.app/api/charts/customer-satisfaction?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&gender=2')
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

**Example:**
```javascript
fetch('https://live-dashboard-backend-production.up.railway.app/api/users/123e4567-e89b-12d3-a456-426614174001/surveys')
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
fetch('https://live-dashboard-backend-production.up.railway.app/api/users/123e4567-e89b-12d3-a456-426614174001/surveys/default')
```

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
```

## 📞 Contact

If you encounter any issues or need additional endpoints, please contact the backend team.


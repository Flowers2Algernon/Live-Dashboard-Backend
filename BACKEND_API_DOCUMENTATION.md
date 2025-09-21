# Live Dashboard Backend API

## 🚀 Base URL
```
https://live-dashboard-backend-production.up.railway.app/api
```
##### participantType means client type
## 📊 Available APIs

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
- `period` (optional): time period filter

**Example:**
```javascript
fetch('https://live-dashboard-backend-production.up.railway.app/api/charts/customer-satisfaction-trend?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&participantType=1')
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

## 📝 Response Format

All APIs return data in this format:

```json
{
  "success": true,
  "message": "Data retrieved successfully",
  "data": {
    // API-specific data structure
  }
}


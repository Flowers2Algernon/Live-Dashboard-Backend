# LERD API Documentation

## Table of Contents
- [Response Chart API](#response-chart-api)
- [Future APIs](#future-apis)

## Response Chart API

### Overview
The Response Chart API retrieves survey response chart data, including total participant count, response rate, and regional participant distribution.

## API Details

### Get Response Chart Data

**Endpoint:** `GET /api/charts/response`

**Description:** Returns response chart data based on survey ID and optional filter conditions.

### Request Parameters

| Parameter | Type | Required | Description | Example |
|-----------|------|----------|-------------|---------|
| `surveyId` | `string (UUID)` | Yes | Survey ID | `8dff523d-2a46-4ee3-8017-614af3813b32` |
| `gender` | `string` | No | Gender filter | `1` (Male), `2` (Female) |
| `participantType` | `string` | No | Participant type filter | `1`, `2`, `3`, etc. |
| `period` | `string` | No | Time period filter | Currently not implemented |

### Request Examples

```bash
# Get all participant data
GET /api/charts/response?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32

# Filter by gender
GET /api/charts/response?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&gender=1

# Filter by gender and participant type
GET /api/charts/response?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&gender=1&participantType=1
```

### Response Format

```json
{
  "success": boolean,
  "message": string,
  "data": {
    "totalParticipants": number,
    "responseRate": string,
    "showRegions": boolean,
    "regions": [
      {
        "villageName": string,
        "participantCount": number
      }
    ]
  }
}
```

### Response Field Descriptions

| Field Name | Type | Description |
|------------|------|-------------|
| `success` | `boolean` | Whether the request was successful |
| `message` | `string` | Error message (empty string on success) |
| `data.totalParticipants` | `number` | Total number of participants |
| `data.responseRate` | `string` | Response rate (currently fixed at "23%") |
| `data.showRegions` | `boolean` | Whether to display regional data |
| `data.regions` | `array` | Array of regional data |
| `data.regions[].villageName` | `string` | Village/region name |
| `data.regions[].participantCount` | `number` | Number of participants in this region |

### Region Mapping

The system supports the following facility code to region name mappings:

| Facility Code | Region Name |
|---------------|-------------|
| `3001` | Bull Creek |
| `3002` | Coolbellup |
| `3003` | Mosman Park |
| `3004` | RoleyStone |
| `3005` | South Perth |
| `3008` | Duncraig |

### Response Examples

#### Successful Response - All Participant Data

```json
{
  "success": true,
  "message": "",
  "data": {
    "totalParticipants": 170,
    "responseRate": "23%",
    "showRegions": true,
    "regions": [
      {
        "villageName": "Bull Creek",
        "participantCount": 32
      },
      {
        "villageName": "Coolbellup",
        "participantCount": 18
      },
      {
        "villageName": "Mosman Park",
        "participantCount": 25
      },
      {
        "villageName": "RoleyStone",
        "participantCount": 32
      },
      {
        "villageName": "South Perth",
        "participantCount": 33
      },
      {
        "villageName": "Duncraig",
        "participantCount": 30
      }
    ]
  }
}
```

#### Successful Response - Filtered Data

```json
{
  "success": true,
  "message": "",
  "data": {
    "totalParticipants": 46,
    "responseRate": "23%",
    "showRegions": true,
    "regions": [
      {
        "villageName": "Bull Creek",
        "participantCount": 10
      },
      {
        "villageName": "Coolbellup",
        "participantCount": 5
      },
      {
        "villageName": "Mosman Park",
        "participantCount": 4
      },
      {
        "villageName": "RoleyStone",
        "participantCount": 11
      },
      {
        "villageName": "South Perth",
        "participantCount": 8
      },
      {
        "villageName": "Duncraig",
        "participantCount": 8
      }
    ]
  }
}
```

#### Error Response Examples

```json
{
  "success": false,
  "message": "Valid survey_id is required",
  "data": null
}
```

```json
{
  "success": false,
  "message": "Internal server error",
  "data": null
}
```

### HTTP Status Codes

| Status Code | Description |
|-------------|-------------|
| `200` | Request successful |
| `400` | Bad request (e.g., missing required surveyId) |
| `500` | Internal server error |

### Frontend Integration Examples

#### JavaScript (Fetch API)

```javascript
async function getResponseChartData(surveyId, filters = {}) {
  const params = new URLSearchParams({
    surveyId: surveyId,
    ...filters
  });
  
  try {
    const response = await fetch(`/api/charts/response?${params}`);
    const result = await response.json();
    
    if (result.success) {
      console.log('Total participants:', result.data.totalParticipants);
      console.log('Response rate:', result.data.responseRate);
      console.log('Regions:', result.data.regions);
      return result.data;
    } else {
      console.error('API Error:', result.message);
      return null;
    }
  } catch (error) {
    console.error('Request failed:', error);
    return null;
  }
}

// Usage examples
getResponseChartData('8dff523d-2a46-4ee3-8017-614af3813b32');
getResponseChartData('8dff523d-2a46-4ee3-8017-614af3813b32', { gender: '1' });
getResponseChartData('8dff523d-2a46-4ee3-8017-614af3813b32', { gender: '1', participantType: '1' });
```

#### React Component Example

```jsx
import React, { useState, useEffect } from 'react';

function ResponseChart({ surveyId, filters }) {
  const [chartData, setChartData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    async function fetchData() {
      setLoading(true);
      setError(null);
      
      const params = new URLSearchParams({
        surveyId: surveyId,
        ...filters
      });
      
      try {
        const response = await fetch(`/api/charts/response?${params}`);
        const result = await response.json();
        
        if (result.success) {
          setChartData(result.data);
        } else {
          setError(result.message);
        }
      } catch (err) {
        setError('Failed to fetch data');
      } finally {
        setLoading(false);
      }
    }

    if (surveyId) {
      fetchData();
    }
  }, [surveyId, filters]);

  if (loading) return <div>Loading...</div>;
  if (error) return <div>Error: {error}</div>;
  if (!chartData) return <div>No data available</div>;

  return (
    <div>
      <h3>Response Chart</h3>
      <p>Total Participants: {chartData.totalParticipants}</p>
      <p>Response Rate: {chartData.responseRate}</p>
      
      {chartData.showRegions && (
        <div>
          <h4>Regional Distribution</h4>
          <ul>
            {chartData.regions.map((region, index) => (
              <li key={index}>
                {region.villageName}: {region.participantCount} participants
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}

export default ResponseChart;
```

### Important Notes

1. **Required Parameters**: `surveyId` is required and must be a valid UUID format
2. **Filters**: All filter parameters are optional; omitting them returns all data
3. **Region Display**: `showRegions` will only be `true` when regional data exists - the doc says only region >0 and < 5 can display, but now we have 6, so i set it as always be true when > 0
4. **Response Rate**: Currently fixed at "23%" in this version
5. **Error Handling**: Frontend should always check the `success` field to determine if the request was successful

### Testing Commands

```bash
# cURL test examples
curl -X GET "http://localhost:5153/api/charts/response?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32" \
  -H "accept: application/json"

curl -X GET "http://localhost:5153/api/charts/response?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&gender=1&participantType=1" \
  -H "accept: application/json"
```

---

## Future APIs

This section will be expanded as additional APIs are implemented:

- **User Management API** - Coming soon
- **Survey Management API** - Coming soon
- **Organization API** - Coming soon
- **Subscription API** - Coming soon

---

## Global API Standards

### Authentication
*To be implemented in future versions*

### Rate Limiting
*To be implemented in future versions*

### Error Codes
All APIs follow the standard error response format:

```json
{
  "success": false,
  "message": "Error description",
  "data": null
}
```

### Base URL
- **Development**: `http://localhost:5153` - temporary use only
- **Production**: *To be configured*

### Content Type
All requests and responses use `application/json` content type.

# LERD API Documentation

## Table of Contents
- [Response Chart API](#response-chart-api)
- [Customer Satisfaction API](#customer-satisfaction-api)
- [Customer Satisfaction Trend API](#customer-satisfaction-trend-api)
- [NPS (Net Promoter Score) API](#nps-net-promoter-score-api)
- [Service Attribute API](#service-attribute-api)
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

## Customer Satisfaction Trend API

### Overview
The Customer Satisfaction Trend API retrieves historical customer satisfaction data across multiple years, combining static historical data with real-time database data.

### Get Customer Satisfaction Trend Data

**Endpoint:** `GET /api/charts/customer-satisfaction-trend`

**Description:** Returns customer satisfaction trend data across years based on survey ID and optional filter conditions.

### Request Parameters

| Parameter | Type | Required | Description | Example |
|-----------|------|----------|-------------|---------|
| `surveyId` | `string (UUID)` | Yes | Survey ID | `8dff523d-2a46-4ee3-8017-614af3813b32` |
| `gender` | `string` | No | Gender filter | `1` (Male), `2` (Female) |
| `participantType` | `string` | No | Participant type filter | `1`, `2`, `3`, etc. |
| `period` | `string` | No | Time period filter | Currently not implemented |

### Request Examples

```bash
# Get all participant trend data
GET /api/charts/customer-satisfaction-trend?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32

# Filter by gender
GET /api/charts/customer-satisfaction-trend?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&gender=1

# Filter by gender and participant type
GET /api/charts/customer-satisfaction-trend?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&gender=1&participantType=1
```

### Response Format

```json
{
  "success": boolean,
  "message": string,
  "data": {
    "years": [
      {
        "year": number,
        "verySatisfiedPercentage": number,
        "satisfiedPercentage": number,
        "somewhatSatisfiedPercentage": number,
        "totalSatisfiedPercentage": number
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
| `data.years` | `array` | Array of yearly satisfaction data |
| `data.years[].year` | `number` | Year of the data |
| `data.years[].verySatisfiedPercentage` | `number` | Percentage of very satisfied customers (satisfaction code 6) |
| `data.years[].satisfiedPercentage` | `number` | Percentage of satisfied customers (satisfaction code 5) |
| `data.years[].somewhatSatisfiedPercentage` | `number` | Percentage of somewhat satisfied customers (satisfaction code 4) |
| `data.years[].totalSatisfiedPercentage` | `number` | Total percentage of satisfied customers (codes 4, 5, 6) |

### Data Sources

| Year | Data Source | Filter Impact |
|------|-------------|---------------|
| `2023` | Static historical data | ❌ No (fixed values) |
| `2024` | Static historical data | ❌ No (fixed values) |
| `2025+` | Real database data | ✅ Yes (filtered dynamically) |

**Note**: Historical data (2023-2024) uses predefined static values for consistency. Real-time data is extracted from the `survey_responses` table based on the `EndDate` field.

### Response Examples

#### Successful Response - All Participant Data

```json
{
  "success": true,
  "message": "",
  "data": {
    "years": [
      {
        "year": 2023,
        "verySatisfiedPercentage": 13,
        "satisfiedPercentage": 32,
        "somewhatSatisfiedPercentage": 38,
        "totalSatisfiedPercentage": 83
      },
      {
        "year": 2024,
        "verySatisfiedPercentage": 36,
        "satisfiedPercentage": 40,
        "somewhatSatisfiedPercentage": 22,
        "totalSatisfiedPercentage": 98
      },
      {
        "year": 2025,
        "verySatisfiedPercentage": 11.8,
        "satisfiedPercentage": 17.6,
        "somewhatSatisfiedPercentage": 17.1,
        "totalSatisfiedPercentage": 46.5
      }
    ]
  }
}
```

#### Successful Response - Filtered Data (Gender=1)

```json
{
  "success": true,
  "message": "",
  "data": {
    "years": [
      {
        "year": 2023,
        "verySatisfiedPercentage": 13,
        "satisfiedPercentage": 32,
        "somewhatSatisfiedPercentage": 38,
        "totalSatisfiedPercentage": 83
      },
      {
        "year": 2024,
        "verySatisfiedPercentage": 36,
        "satisfiedPercentage": 40,
        "somewhatSatisfiedPercentage": 22,
        "totalSatisfiedPercentage": 98
      },
      {
        "year": 2025,
        "verySatisfiedPercentage": 8.4,
        "satisfiedPercentage": 16.9,
        "somewhatSatisfiedPercentage": 20.5,
        "totalSatisfiedPercentage": 45.8
      }
    ]
  }
}
```

### Frontend Integration Example

```javascript
async function getCustomerSatisfactionTrendData(surveyId, filters = {}) {
  const params = new URLSearchParams({
    surveyId: surveyId,
    ...filters
  });
  
  try {
    const response = await fetch(`/api/charts/customer-satisfaction-trend?${params}`);
    const result = await response.json();
    
    if (result.success) {
      console.log('Trend data for', result.data.years.length, 'years:');
      result.data.years.forEach(year => {
        console.log(`${year.year}: ${year.totalSatisfiedPercentage}% satisfied`);
      });
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
getCustomerSatisfactionTrendData('8dff523d-2a46-4ee3-8017-614af3813b32');
getCustomerSatisfactionTrendData('8dff523d-2a46-4ee3-8017-614af3813b32', { gender: '1' });
```

### Chart.js Integration Example

```javascript
// Example of how to use the trend data with Chart.js
function createTrendChart(trendData) {
  const ctx = document.getElementById('trendChart').getContext('2d');
  
  const chart = new Chart(ctx, {
    type: 'line',
    data: {
      labels: trendData.years.map(y => y.year.toString()),
      datasets: [
        {
          label: 'Very Satisfied',
          data: trendData.years.map(y => y.verySatisfiedPercentage),
          borderColor: 'rgb(75, 192, 192)',
          tension: 0.1
        },
        {
          label: 'Satisfied', 
          data: trendData.years.map(y => y.satisfiedPercentage),
          borderColor: 'rgb(54, 162, 235)',
          tension: 0.1
        },
        {
          label: 'Somewhat Satisfied',
          data: trendData.years.map(y => y.somewhatSatisfiedPercentage),
          borderColor: 'rgb(255, 205, 86)',
          tension: 0.1
        }
      ]
    },
    options: {
      responsive: true,
      plugins: {
        title: {
          display: true,
          text: 'Customer Satisfaction Trend'
        }
      },
      scales: {
        y: {
          beginAtZero: true,
          max: 100,
          title: {
            display: true,
            text: 'Percentage (%)'
          }
        }
      }
    }
  });
}
```

### Testing Commands

```bash
# cURL test examples
curl -X GET "http://localhost:5153/api/charts/customer-satisfaction-trend?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32" \
  -H "accept: application/json"

curl -X GET "http://localhost:5153/api/charts/customer-satisfaction-trend?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&gender=1" \
  -H "accept: application/json"
```

---

## NPS (Net Promoter Score) API

### Overview
The NPS API provides a comprehensive Net Promoter Score analysis, returning both the calculated NPS score and detailed distribution data in a single endpoint. This unified API follows efficient database querying practices and shared filter logic.

## API Details

### Get NPS Data

**Endpoint:** `GET /api/charts/nps`

**Description:** Returns Net Promoter Score and distribution data based on survey ID and optional filter conditions. The API calculates the NPS score using the standard formula: `(Promoters - Detractors) / Total * 100`.

### Request Parameters

| Parameter | Type | Required | Description | Example |
|-----------|------|----------|-------------|---------|
| `surveyId` | `string (UUID)` | Yes | Survey ID | `8dff523d-2a46-4ee3-8017-614af3813b32` |
| `gender` | `string` | No | Gender filter | `1` (Male), `2` (Female) |
| `participantType` | `string` | No | Participant type filter | `1`, `2`, `3`, etc. |
| `period` | `string` | No | Time period filter | Currently not implemented |

### Request Examples

```bash
# Get all NPS data
GET /api/charts/nps?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32

# Filter by gender (male participants only)
GET /api/charts/nps?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&gender=1

# Filter by gender (female participants only)
GET /api/charts/nps?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&gender=2

# Filter by both gender and participant type
GET /api/charts/nps?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&gender=1&participantType=1
```

### Response Format

```json
{
  "success": true,
  "message": "NPS data retrieved successfully",
  "data": {
    "npsScore": 25,
    "distribution": {
      "promoterCount": 150,
      "passiveCount": 75,
      "detractorCount": 100,
      "totalCount": 325,
      "promoterPercentage": 46.2,
      "passivePercentage": 23.1,
      "detractorPercentage": 30.8
    }
  }
}
```

### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `npsScore` | `integer` | Net Promoter Score (-100 to 100) |
| `distribution.promoterCount` | `integer` | Number of promoters (score 9-10) |
| `distribution.passiveCount` | `integer` | Number of passive respondents (score 7-8) |
| `distribution.detractorCount` | `integer` | Number of detractors (score 0-6) |
| `distribution.totalCount` | `integer` | Total number of respondents |
| `distribution.promoterPercentage` | `decimal` | Percentage of promoters (rounded to 1 decimal) |
| `distribution.passivePercentage` | `decimal` | Percentage of passive respondents (rounded to 1 decimal) |
| `distribution.detractorPercentage` | `decimal` | Percentage of detractors (rounded to 1 decimal) |

### NPS Score Calculation

The NPS score is calculated using the standard formula:
```
NPS Score = ((Promoters - Detractors) / Total Respondents) × 100
```

- **Promoters**: Respondents with NPS group `3` (typically scores 9-10)
- **Passive**: Respondents with NPS group `2` (typically scores 7-8)  
- **Detractors**: Respondents with NPS group `1` (typically scores 0-6)

### Example Response Data

#### All Participants
```json
{
  "success": true,
  "message": "NPS data retrieved successfully",
  "data": {
    "npsScore": 25,
    "distribution": {
      "promoterCount": 150,
      "passiveCount": 75,
      "detractorCount": 100,
      "totalCount": 325,
      "promoterPercentage": 46.2,
      "passivePercentage": 23.1,
      "detractorPercentage": 30.8
    }
  }
}
```

#### Male Participants Only (gender=1)
```json
{
  "success": true,
  "message": "NPS data retrieved successfully",
  "data": {
    "npsScore": 30,
    "distribution": {
      "promoterCount": 85,
      "passiveCount": 40,
      "detractorCount": 50,
      "totalCount": 175,
      "promoterPercentage": 48.6,
      "passivePercentage": 22.9,
      "detractorPercentage": 28.6
    }
  }
}
```

#### Female Participants Only (gender=2)
```json
{
  "success": true,
  "message": "NPS data retrieved successfully",
  "data": {
    "npsScore": 20,
    "distribution": {
      "promoterCount": 65,
      "passiveCount": 35,
      "detractorCount": 50,
      "totalCount": 150,
      "promoterPercentage": 43.3,
      "passivePercentage": 23.3,
      "detractorPercentage": 33.3
    }
  }
}
```

### Error Responses

#### Invalid Survey ID
```json
{
  "success": false,
  "message": "Invalid survey ID format",
  "data": null
}
```

#### No Data Found
```json
{
  "success": true,
  "message": "NPS data retrieved successfully",
  "data": {
    "npsScore": 0,
    "distribution": {
      "promoterCount": 0,
      "passiveCount": 0,
      "detractorCount": 0,
      "totalCount": 0,
      "promoterPercentage": 0.0,
      "passivePercentage": 0.0,
      "detractorPercentage": 0.0
    }
  }
}
```

#### Server Error
```json
{
  "success": false,
  "message": "An error occurred while getting NPS data",
  "data": null
}
```

## Frontend Integration Examples

### JavaScript/Fetch
```javascript
// Get NPS data for all participants
async function getNPSData(surveyId) {
    try {
        const response = await fetch(`/api/charts/nps?surveyId=${surveyId}`);
        const result = await response.json();
        
        if (result.success) {
            const { npsScore, distribution } = result.data;
            console.log(`NPS Score: ${npsScore}`);
            console.log(`Total Responses: ${distribution.totalCount}`);
            console.log(`Promoters: ${distribution.promoterCount} (${distribution.promoterPercentage}%)`);
            console.log(`Passive: ${distribution.passiveCount} (${distribution.passivePercentage}%)`);
            console.log(`Detractors: ${distribution.detractorCount} (${distribution.detractorPercentage}%)`);
            return result.data;
        } else {
            console.error('Error:', result.message);
        }
    } catch (error) {
        console.error('Network error:', error);
    }
}

// Get filtered NPS data
async function getFilteredNPSData(surveyId, filters = {}) {
    const params = new URLSearchParams({ surveyId });
    
    if (filters.gender) params.append('gender', filters.gender);
    if (filters.participantType) params.append('participantType', filters.participantType);
    
    try {
        const response = await fetch(`/api/charts/nps?${params}`);
        const result = await response.json();
        return result.success ? result.data : null;
    } catch (error) {
        console.error('Error fetching NPS data:', error);
        return null;
    }
}

// Usage examples
getNPSData('8dff523d-2a46-4ee3-8017-614af3813b32');
getFilteredNPSData('8dff523d-2a46-4ee3-8017-614af3813b32', { gender: '1' });
getFilteredNPSData('8dff523d-2a46-4ee3-8017-614af3813b32', { gender: '2', participantType: '1' });
```

### React Hook Example
```jsx
import { useState, useEffect } from 'react';

function useNPSData(surveyId, filters = {}) {
    const [npsData, setNPSData] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        async function fetchNPSData() {
            setLoading(true);
            setError(null);
            
            try {
                const params = new URLSearchParams({ surveyId });
                if (filters.gender) params.append('gender', filters.gender);
                if (filters.participantType) params.append('participantType', filters.participantType);
                
                const response = await fetch(`/api/charts/nps?${params}`);
                const result = await response.json();
                
                if (result.success) {
                    setNPSData(result.data);
                } else {
                    setError(result.message);
                }
            } catch (err) {
                setError('Failed to fetch NPS data');
            } finally {
                setLoading(false);
            }
        }

        if (surveyId) {
            fetchNPSData();
        }
    }, [surveyId, filters.gender, filters.participantType]);

    return { npsData, loading, error };
}

// Component usage
function NPSChart({ surveyId, filters }) {
    const { npsData, loading, error } = useNPSData(surveyId, filters);

    if (loading) return <div>Loading NPS data...</div>;
    if (error) return <div>Error: {error}</div>;
    if (!npsData) return <div>No NPS data available</div>;

    return (
        <div className="nps-chart">
            <h3>Net Promoter Score: {npsData.npsScore}</h3>
            <div className="nps-distribution">
                <div>Promoters: {npsData.distribution.promoterCount} ({npsData.distribution.promoterPercentage}%)</div>
                <div>Passive: {npsData.distribution.passiveCount} ({npsData.distribution.passivePercentage}%)</div>
                <div>Detractors: {npsData.distribution.detractorCount} ({npsData.distribution.detractorPercentage}%)</div>
                <div>Total: {npsData.distribution.totalCount}</div>
            </div>
        </div>
    );
}
```

### Chart.js Integration Example
```javascript
// Create NPS distribution chart using Chart.js
function createNPSChart(npsData) {
    const ctx = document.getElementById('npsChart').getContext('2d');
    
    return new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: ['Promoters', 'Passive', 'Detractors'],
            datasets: [{
                data: [
                    npsData.distribution.promoterCount,
                    npsData.distribution.passiveCount,
                    npsData.distribution.detractorCount
                ],
                backgroundColor: [
                    '#4CAF50', // Green for promoters
                    '#FFC107', // Yellow for passive
                    '#F44336'  // Red for detractors
                ],
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            plugins: {
                title: {
                    display: true,
                    text: `NPS Score: ${npsData.npsScore}`
                },
                legend: {
                    position: 'bottom'
                }
            }
        }
    });
}
```

## Testing Instructions

### Manual Testing

1. **Test all participants:**
   ```bash
   curl "http://localhost:5153/api/charts/nps?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32"
   ```

2. **Test gender filter (male):**
   ```bash
   curl "http://localhost:5153/api/charts/nps?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&gender=1"
   ```

3. **Test gender filter (female):**
   ```bash
   curl "http://localhost:5153/api/charts/nps?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&gender=2"
   ```

4. **Test combined filters:**
   ```bash
   curl "http://localhost:5153/api/charts/nps?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&gender=1&participantType=1"
   ```

5. **Test invalid survey ID:**
   ```bash
   curl "http://localhost:5153/api/charts/nps?surveyId=invalid-id"
   ```

### Expected Results

- **All participants**: Should return overall NPS score and distribution
- **Gender filters**: Should return filtered data with appropriate counts and percentages
- **Combined filters**: Should apply all filters simultaneously
- **Invalid survey ID**: Should return a 400 error with appropriate message
- **No data**: Should return zero values with success status

### Verification Points

1. **NPS Score Calculation**: Verify that `(promoterCount - detractorCount) / totalCount * 100` equals the returned NPS score
2. **Percentage Calculation**: Verify that percentages sum to 100% (allowing for rounding)
3. **Count Consistency**: Verify that `promoterCount + passiveCount + detractorCount = totalCount`
4. **Filter Effectiveness**: Verify that applying filters reduces the total count appropriately
5. **Response Format**: Verify all required fields are present and have correct data types

---

## Service Attribute API

### Overview
The Service Attribute API provides a comprehensive analysis of service attributes with dynamic attribute detection and chart-level filtering. This is the most complex chart API as it supports both traditional demographic filters and dynamic attribute selection filters. The API automatically detects available attributes from survey data and allows filtering to specific attributes of interest.

## API Details

### Get Service Attribute Data

**Endpoint:** `GET /api/charts/service-attributes`

**Description:** Returns service attribute analysis data with dynamic attribute detection, supporting both demographic filters and chart-level attribute selection filters. The API analyzes attributes starting with `Ab_` prefix in survey response data.

### Request Parameters

| Parameter | Type | Required | Description | Example |
|-----------|------|----------|-------------|---------|
| `surveyId` | `string (UUID)` | Yes | Survey ID | `8dff523d-2a46-4ee3-8017-614af3813b32` |
| `gender` | `string` | No | Gender filter | `1` (Male), `2` (Female) |
| `participantType` | `string` | No | Participant type filter | `1`, `2`, `3`, etc. |
| `period` | `string` | No | Time period filter | Currently not implemented |
| `selectedAttributes` | `string[]` | No | Chart-level attribute filter | `["Safety", "Activities"]` |

### Dynamic Attribute Detection

The API automatically detects available attributes from survey data by scanning for fields with the `Ab_` prefix:

| Database Field | Mapped Display Name |
|----------------|-------------------|
| `Ab_Safety` | "Safety & Security" |
| `Ab_Location` | "Village Location Access" |
| `Ab_Activities` | "Activity Availability" |
| `Ab_Facilities` | "Facilities" |
| `Ab_Garden care` | "Garden Care" |
| `Ab_Staff service` | "Staff Service" |

### Value Mapping

Service attribute responses use a 4-point scale:

| Database Value | Meaning |
|---------------|---------|
| `"1"` | "Never" |
| `"2"` | "Some of the time" |
| `"3"` | "Most of the time" |
| `"4"` | "Always" |

### Request Examples

```bash
# Get all service attribute data
GET /api/charts/service-attributes?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32

# Filter by gender (male participants only)
GET /api/charts/service-attributes?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&gender=1

# Filter by gender (female participants only)
GET /api/charts/service-attributes?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&gender=2

# Filter to specific attributes only
GET /api/charts/service-attributes?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&selectedAttributes=Safety&selectedAttributes=Activities

# Combined demographic and attribute filtering
GET /api/charts/service-attributes?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&gender=1&selectedAttributes=Safety&selectedAttributes=Facilities
```

### Response Format

```json
{
  "success": true,
  "message": "Service attribute data retrieved successfully",
  "data": {
    "attributes": [
      {
        "attributeName": "Safety & Security",
        "totalResponses": 170,
        "validResponses": 170,
        "alwaysCount": 47,
        "mostCount": 39,
        "alwaysPercentage": 27.6,
        "mostPercentage": 22.9,
        "criteria80Percentage": 80.0,
        "criteria60Percentage": 60.0
      }
    ],
    "availableAttributes": ["Safety", "Activities", "Facilities", "Garden care", "Location", "Staff service"]
  }
}
```

### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `attributes` | `array` | Array of attribute analysis results |
| `attributes[].attributeName` | `string` | Display name of the attribute |
| `attributes[].totalResponses` | `integer` | Total number of survey responses |
| `attributes[].validResponses` | `integer` | Number of valid responses for this attribute |
| `attributes[].alwaysCount` | `integer` | Number of "Always" responses (value "4") |
| `attributes[].mostCount` | `integer` | Number of "Most of the time" responses (value "3") |
| `attributes[].alwaysPercentage` | `decimal` | Percentage of "Always" responses (rounded to 1 decimal) |
| `attributes[].mostPercentage` | `decimal` | Percentage of "Most of the time" responses (rounded to 1 decimal) |
| `attributes[].criteria80Percentage` | `decimal` | 80% benchmark line (fixed at 80.0) |
| `attributes[].criteria60Percentage` | `decimal` | 60% benchmark line (fixed at 60.0) |
| `availableAttributes` | `array` | List of all available attributes for filtering |

### Calculation Logic

- **Always Percentage**: `(alwaysCount / validResponses) × 100`
- **Most Percentage**: `(mostCount / validResponses) × 100`
- **Combined Satisfaction**: `alwaysPercentage + mostPercentage` (represents satisfied customers)
- **Benchmark Comparison**: Results can be compared against 80% and 60% criteria lines

### Example Response Data

#### All Participants - All Attributes
```json
{
  "success": true,
  "message": "Service attribute data retrieved successfully",
  "data": {
    "attributes": [
      {
        "attributeName": "Safety & Security",
        "totalResponses": 170,
        "validResponses": 170,
        "alwaysCount": 47,
        "mostCount": 39,
        "alwaysPercentage": 27.6,
        "mostPercentage": 22.9,
        "criteria80Percentage": 80.0,
        "criteria60Percentage": 60.0
      },
      {
        "attributeName": "Activity Availability",
        "totalResponses": 170,
        "validResponses": 170,
        "alwaysCount": 45,
        "mostCount": 44,
        "alwaysPercentage": 26.5,
        "mostPercentage": 25.9,
        "criteria80Percentage": 80.0,
        "criteria60Percentage": 60.0
      },
      {
        "attributeName": "Facilities",
        "totalResponses": 170,
        "validResponses": 170,
        "alwaysCount": 41,
        "mostCount": 42,
        "alwaysPercentage": 24.1,
        "mostPercentage": 24.7,
        "criteria80Percentage": 80.0,
        "criteria60Percentage": 60.0
      }
    ],
    "availableAttributes": ["Safety", "Activities", "Facilities", "Garden care", "Location", "Staff service"]
  }
}
```

#### Male Participants Only (gender=1)
```json
{
  "success": true,
  "message": "Service attribute data retrieved successfully",
  "data": {
    "attributes": [
      {
        "attributeName": "Safety & Security",
        "totalResponses": 83,
        "validResponses": 83,
        "alwaysCount": 23,
        "mostCount": 21,
        "alwaysPercentage": 27.7,
        "mostPercentage": 25.3,
        "criteria80Percentage": 80.0,
        "criteria60Percentage": 60.0
      },
      {
        "attributeName": "Activity Availability",
        "totalResponses": 83,
        "validResponses": 83,
        "alwaysCount": 26,
        "mostCount": 22,
        "alwaysPercentage": 31.3,
        "mostPercentage": 26.5,
        "criteria80Percentage": 80.0,
        "criteria60Percentage": 60.0
      }
    ],
    "availableAttributes": ["Safety", "Activities", "Facilities", "Garden care", "Location", "Staff service"]
  }
}
```

#### Selected Attributes Only
```json
{
  "success": true,
  "message": "Service attribute data retrieved successfully",
  "data": {
    "attributes": [
      {
        "attributeName": "Safety & Security",
        "totalResponses": 170,
        "validResponses": 170,
        "alwaysCount": 47,
        "mostCount": 39,
        "alwaysPercentage": 27.6,
        "mostPercentage": 22.9,
        "criteria80Percentage": 80.0,
        "criteria60Percentage": 60.0
      },
      {
        "attributeName": "Activity Availability",
        "totalResponses": 170,
        "validResponses": 170,
        "alwaysCount": 45,
        "mostCount": 44,
        "alwaysPercentage": 26.5,
        "mostPercentage": 25.9,
        "criteria80Percentage": 80.0,
        "criteria60Percentage": 60.0
      }
    ],
    "availableAttributes": ["Safety", "Activities", "Facilities", "Garden care", "Location", "Staff service"]
  }
}
```

### Error Responses

#### Invalid Survey ID
```json
{
  "success": false,
  "message": "Valid survey_id is required",
  "data": null
}
```

#### No Data Found
```json
{
  "success": true,
  "message": "Service attribute data retrieved successfully",
  "data": {
    "attributes": [],
    "availableAttributes": []
  }
}
```

#### Server Error
```json
{
  "success": false,
  "message": "An error occurred while getting service attribute data",
  "data": null
}
```

## Frontend Integration Examples

### JavaScript/Fetch
```javascript
// Get all service attribute data
async function getServiceAttributeData(surveyId) {
    try {
        const response = await fetch(`/api/charts/service-attributes?surveyId=${surveyId}`);
        const result = await response.json();
        
        if (result.success) {
            console.log(`Found ${result.data.attributes.length} attributes`);
            result.data.attributes.forEach(attr => {
                const combinedSatisfaction = attr.alwaysPercentage + attr.mostPercentage;
                console.log(`${attr.attributeName}: ${combinedSatisfaction.toFixed(1)}% satisfied (${attr.alwaysCount + attr.mostCount}/${attr.validResponses})`);
            });
            return result.data;
        } else {
            console.error('Error:', result.message);
        }
    } catch (error) {
        console.error('Network error:', error);
    }
}

// Get filtered service attribute data
async function getFilteredServiceAttributeData(surveyId, options = {}) {
    const params = new URLSearchParams({ surveyId });
    
    if (options.gender) params.append('gender', options.gender);
    if (options.participantType) params.append('participantType', options.participantType);
    if (options.selectedAttributes) {
        options.selectedAttributes.forEach(attr => params.append('selectedAttributes', attr));
    }
    
    try {
        const response = await fetch(`/api/charts/service-attributes?${params}`);
        const result = await response.json();
        return result.success ? result.data : null;
    } catch (error) {
        console.error('Error fetching service attribute data:', error);
        return null;
    }
}

// Usage examples
getServiceAttributeData('8dff523d-2a46-4ee3-8017-614af3813b32');
getFilteredServiceAttributeData('8dff523d-2a46-4ee3-8017-614af3813b32', { gender: '1' });
getFilteredServiceAttributeData('8dff523d-2a46-4ee3-8017-614af3813b32', { 
    selectedAttributes: ['Safety', 'Activities'] 
});
getFilteredServiceAttributeData('8dff523d-2a46-4ee3-8017-614af3813b32', { 
    gender: '1', 
    selectedAttributes: ['Safety', 'Facilities'] 
});
```

### React Hook Example
```jsx
import { useState, useEffect } from 'react';

function useServiceAttributeData(surveyId, filters = {}) {
    const [attributeData, setAttributeData] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        async function fetchAttributeData() {
            setLoading(true);
            setError(null);
            
            try {
                const params = new URLSearchParams({ surveyId });
                if (filters.gender) params.append('gender', filters.gender);
                if (filters.participantType) params.append('participantType', filters.participantType);
                if (filters.selectedAttributes) {
                    filters.selectedAttributes.forEach(attr => params.append('selectedAttributes', attr));
                }
                
                const response = await fetch(`/api/charts/service-attributes?${params}`);
                const result = await response.json();
                
                if (result.success) {
                    setAttributeData(result.data);
                } else {
                    setError(result.message);
                }
            } catch (err) {
                setError('Failed to fetch service attribute data');
            } finally {
                setLoading(false);
            }
        }

        if (surveyId) {
            fetchAttributeData();
        }
    }, [surveyId, filters.gender, filters.participantType, JSON.stringify(filters.selectedAttributes)]);

    return { attributeData, loading, error };
}

// Component usage
function ServiceAttributeChart({ surveyId, filters }) {
    const { attributeData, loading, error } = useServiceAttributeData(surveyId, filters);

    if (loading) return <div>Loading service attribute data...</div>;
    if (error) return <div>Error: {error}</div>;
    if (!attributeData) return <div>No service attribute data available</div>;

    return (
        <div className="service-attribute-chart">
            <h3>Service Attributes Analysis</h3>
            <div className="attribute-list">
                {attributeData.attributes.map((attr, index) => {
                    const combinedSatisfaction = attr.alwaysPercentage + attr.mostPercentage;
                    const meetsTarget = combinedSatisfaction >= 80;
                    
                    return (
                        <div key={index} className={`attribute-item ${meetsTarget ? 'meets-target' : 'below-target'}`}>
                            <h4>{attr.attributeName}</h4>
                            <div className="satisfaction-breakdown">
                                <div>Always: {attr.alwaysCount} ({attr.alwaysPercentage}%)</div>
                                <div>Most of the time: {attr.mostCount} ({attr.mostPercentage}%)</div>
                                <div className="combined-satisfaction">
                                    Combined Satisfaction: {combinedSatisfaction.toFixed(1)}%
                                </div>
                            </div>
                            <div className="progress-bar">
                                <div 
                                    className="progress-fill"
                                    style={{ width: `${Math.min(combinedSatisfaction, 100)}%` }}
                                ></div>
                                <div className="benchmark-line" style={{ left: '80%' }}>80%</div>
                                <div className="benchmark-line" style={{ left: '60%' }}>60%</div>
                            </div>
                        </div>
                    );
                })}
            </div>
            
            <div className="available-attributes">
                <h4>Available Attributes for Filtering:</h4>
                <div className="attribute-tags">
                    {attributeData.availableAttributes.map((attr, index) => (
                        <span key={index} className="attribute-tag">{attr}</span>
                    ))}
                </div>
            </div>
        </div>
    );
}
```

### Chart.js Integration Example
```javascript
// Create stacked bar chart for service attributes
function createServiceAttributeChart(attributeData) {
    const ctx = document.getElementById('serviceAttributeChart').getContext('2d');
    
    const labels = attributeData.attributes.map(attr => attr.attributeName);
    const alwaysData = attributeData.attributes.map(attr => attr.alwaysPercentage);
    const mostData = attributeData.attributes.map(attr => attr.mostPercentage);
    
    return new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Always',
                    data: alwaysData,
                    backgroundColor: '#4CAF50',
                    stack: 'satisfaction'
                },
                {
                    label: 'Most of the time',
                    data: mostData,
                    backgroundColor: '#8BC34A',
                    stack: 'satisfaction'
                }
            ]
        },
        options: {
            responsive: true,
            plugins: {
                title: {
                    display: true,
                    text: 'Service Attributes Satisfaction'
                },
                legend: {
                    position: 'top'
                }
            },
            scales: {
                x: {
                    stacked: true,
                    title: {
                        display: true,
                        text: 'Service Attributes'
                    }
                },
                y: {
                    stacked: true,
                    beginAtZero: true,
                    max: 100,
                    title: {
                        display: true,
                        text: 'Percentage (%)'
                    }
                }
            },
            annotation: {
                annotations: {
                    benchmark80: {
                        type: 'line',
                        yMin: 80,
                        yMax: 80,
                        borderColor: 'red',
                        borderWidth: 2,
                        label: {
                            content: '80% Target',
                            enabled: true,
                            position: 'end'
                        }
                    },
                    benchmark60: {
                        type: 'line',
                        yMin: 60,
                        yMax: 60,
                        borderColor: 'orange',
                        borderWidth: 1,
                        label: {
                            content: '60% Minimum',
                            enabled: true,
                            position: 'end'
                        }
                    }
                }
            }
        }
    });
}
```

## Testing Instructions

### Manual Testing

1. **Test all attributes data:**
   ```bash
   curl "http://localhost:5153/api/charts/service-attributes?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32"
   ```

2. **Test gender filter (male):**
   ```bash
   curl "http://localhost:5153/api/charts/service-attributes?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&gender=1"
   ```

3. **Test gender filter (female):**
   ```bash
   curl "http://localhost:5153/api/charts/service-attributes?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&gender=2"
   ```

4. **Test selected attributes:**
   ```bash
   curl "http://localhost:5153/api/charts/service-attributes?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&selectedAttributes=Safety&selectedAttributes=Activities"
   ```

5. **Test combined filters:**
   ```bash
   curl "http://localhost:5153/api/charts/service-attributes?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&gender=1&selectedAttributes=Safety&selectedAttributes=Facilities"
   ```

6. **Test invalid survey ID:**
   ```bash
   curl "http://localhost:5153/api/charts/service-attributes?surveyId=invalid-id"
   ```

### Expected Results

- **All attributes**: Should return all 6 service attributes with full participant data
- **Gender filters**: Should return filtered data with reduced participant counts
- **Selected attributes**: Should return only the specified attributes
- **Combined filters**: Should apply both demographic and attribute filters
- **Invalid survey ID**: Should return a 400 error with appropriate message
- **No data**: Should return empty arrays with success status

### Verification Points

1. **Dynamic Attribute Detection**: Verify that `availableAttributes` array contains all attributes found in the survey data
2. **Attribute Name Mapping**: Verify that display names match the mapping (e.g., "Safety" → "Safety & Security")
3. **Percentage Calculation**: Verify that `alwaysPercentage = (alwaysCount / validResponses) × 100`
4. **Filter Effectiveness**: Verify that demographic filters reduce participant counts appropriately
5. **Attribute Selection**: Verify that `selectedAttributes` parameter filters the returned attributes
6. **Response Consistency**: Verify that `totalResponses` matches between all attributes for the same filter set
7. **Benchmark Values**: Verify that criteria percentages are always 80.0 and 60.0

### Advanced Testing Scenarios

```bash
# Test with non-existent attributes (should be ignored)
curl "http://localhost:5153/api/charts/service-attributes?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&selectedAttributes=NonExistent&selectedAttributes=Safety"

# Test with all possible demographic combinations
curl "http://localhost:5153/api/charts/service-attributes?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32&gender=1&participantType=1"

# Test response format with jq for JSON validation
curl "http://localhost:5153/api/charts/service-attributes?surveyId=8dff523d-2a46-4ee3-8017-614af3813b32" | jq '.data.attributes[0] | keys'
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

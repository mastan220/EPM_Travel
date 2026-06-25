import { useState } from 'react'
import './App.css'

type FlightStatus = 'OnTime' | 'Delayed' | 'Cancelled' | 'Diverted' | 'Unknown'

interface FlightStatusResult {
  flightNumber: string
  date: string
  status: FlightStatus
  scheduledDepartureUtc: string | null
  actualDepartureUtc: string | null
  scheduledArrivalUtc: string | null
  actualArrivalUtc: string | null
  terminal: string | null
  gate: string | null
  delayReason: string | null
  providerName: string
  lastUpdatedUtc: string | null
  message: string
}

function App() {
  const [flightNumber, setFlightNumber] = useState('')
  const [date, setDate] = useState('')
  const [result, setResult] = useState<FlightStatusResult | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleSearch = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true)
    setError(null)
    setResult(null)

    try {
      const apiUrl = import.meta.env.VITE_API_URL || 'https://localhost:7070'
      const response = await fetch(
        `${apiUrl}/flights/status?flightNumber=${encodeURIComponent(flightNumber)}&date=${encodeURIComponent(date)}`
      )

      if (!response.ok) {
        const errorData = await response.json()
        setError(errorData.error || 'Failed to fetch flight status')
        return
      }

      const data = await response.json()
      setResult(data)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred')
    } finally {
      setLoading(false)
    }
  }

  const getStatusColor = (status: FlightStatus): string => {
    switch (status) {
      case 'OnTime':
        return '#22c55e' // green
      case 'Delayed':
        return '#f59e0b' // amber
      case 'Cancelled':
      case 'Diverted':
        return '#ef4444' // red
      case 'Unknown':
      default:
        return '#9ca3af' // grey
    }
  }

  const formatDateTime = (utcString: string | null): string => {
    if (!utcString) return 'N/A'
    const date = new Date(utcString)
    return date.toLocaleString()
  }

  return (
    <div className="app">
      <header className="header">
        <h1>Flight Status Tracker</h1>
        <p>Check your flight status quickly</p>
      </header>

      <main className="main">
        <form onSubmit={handleSearch} className="search-form">
          <div className="form-group">
            <label htmlFor="flightNumber">Flight Number</label>
            <input
              id="flightNumber"
              type="text"
              placeholder="e.g., AA100"
              value={flightNumber}
              onChange={(e) => setFlightNumber(e.target.value.toUpperCase())}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="date">Date (yyyy-MM-dd)</label>
            <input
              id="date"
              type="date"
              value={date}
              onChange={(e) => setDate(e.target.value)}
              required
            />
          </div>

          <button type="submit" disabled={loading} className="search-button">
            {loading ? 'Searching...' : 'Search'}
          </button>
        </form>

        {error && (
          <div className="error-state">
            <h3>Error</h3>
            <p>{error}</p>
          </div>
        )}

        {result && (
          <div
            className="result-card"
            style={{
              borderLeftColor: getStatusColor(result.status),
            }}
          >
            <div className="result-header">
              <h2>{result.flightNumber}</h2>
              <div
                className="status-badge"
                style={{
                  backgroundColor: getStatusColor(result.status),
                }}
              >
                {result.status}
              </div>
            </div>

            <div className="result-content">
              <div className="result-section">
                <h3>Flight Details</h3>
                <div className="detail-row">
                  <span className="label">Date:</span>
                  <span className="value">{result.date}</span>
                </div>
                <div className="detail-row">
                  <span className="label">Status:</span>
                  <span className="value" style={{ color: getStatusColor(result.status) }}>
                    {result.status}
                  </span>
                </div>
                {result.providerName && (
                  <div className="detail-row">
                    <span className="label">Provider:</span>
                    <span className="value">{result.providerName}</span>
                  </div>
                )}
                {result.lastUpdatedUtc && (
                  <div className="detail-row">
                    <span className="label">Last Updated:</span>
                    <span className="value">{formatDateTime(result.lastUpdatedUtc)}</span>
                  </div>
                )}
              </div>

              <div className="result-section">
                <h3>Times</h3>
                {result.scheduledDepartureUtc && (
                  <div className="detail-row">
                    <span className="label">Scheduled Departure:</span>
                    <span className="value">{formatDateTime(result.scheduledDepartureUtc)}</span>
                  </div>
                )}
                {result.actualDepartureUtc && (
                  <div className="detail-row">
                    <span className="label">Actual Departure:</span>
                    <span className="value">{formatDateTime(result.actualDepartureUtc)}</span>
                  </div>
                )}
                {result.scheduledArrivalUtc && (
                  <div className="detail-row">
                    <span className="label">Scheduled Arrival:</span>
                    <span className="value">{formatDateTime(result.scheduledArrivalUtc)}</span>
                  </div>
                )}
                {result.actualArrivalUtc && (
                  <div className="detail-row">
                    <span className="label">Actual Arrival:</span>
                    <span className="value">{formatDateTime(result.actualArrivalUtc)}</span>
                  </div>
                )}
              </div>

              {(result.terminal || result.gate || result.delayReason) && (
                <div className="result-section">
                  <h3>Additional Details</h3>
                  {result.terminal && (
                    <div className="detail-row">
                      <span className="label">Terminal:</span>
                      <span className="value">{result.terminal}</span>
                    </div>
                  )}
                  {result.gate && (
                    <div className="detail-row">
                      <span className="label">Gate:</span>
                      <span className="value">{result.gate}</span>
                    </div>
                  )}
                  {result.delayReason && (
                    <div className="detail-row">
                      <span className="label">Delay Reason:</span>
                      <span className="value">{result.delayReason}</span>
                    </div>
                  )}
                </div>
              )}

              {result.message && (
                <div className="message">
                  <p>{result.message}</p>
                </div>
              )}
            </div>
          </div>
        )}
      </main>
    </div>
  )
}

export default App

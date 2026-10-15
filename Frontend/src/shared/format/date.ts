const MONTHS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']

// Split by hand: new Date('2026-10-15') is UTC midnight, which is the previous day west of UTC.
function parts(date: string) {
  const [year, month, day] = date.split('-').map(Number)
  return { year, month, day }
}

/** "2026-10-15" -> "15 Oct 2026" */
export function formatDate(date: string) {
  const { year, month, day } = parts(date)
  return `${day} ${MONTHS[month - 1]} ${year}`
}

/** "2026-10-15" -> "15 Oct" */
export function formatShortDate(date: string) {
  const { month, day } = parts(date)
  return `${day} ${MONTHS[month - 1]}`
}

/** Today in local time as "YYYY-MM-DD". */
export function today() {
  const now = new Date()
  const month = String(now.getMonth() + 1).padStart(2, '0')
  const day = String(now.getDate()).padStart(2, '0')
  return `${now.getFullYear()}-${month}-${day}`
}

const currency = new Intl.NumberFormat('en-AU', { style: 'currency', currency: 'AUD' })

/** 1234.5 -> "$1,234.50" */
export function formatMoney(amount: number) {
  return currency.format(amount)
}

/**
 * What a money field holds ("$1,234.50", "12.5") -> a number for the API.
 * Extra decimals are kept so the server can reject them; empty or unreadable -> NaN.
 */
export function parseMoney(text: string) {
  const cleaned = text.trim().replace(/^\$/, '').replaceAll(',', '')
  return cleaned === '' ? NaN : Number(cleaned)
}

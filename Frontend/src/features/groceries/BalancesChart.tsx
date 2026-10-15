import { Bar, BarChart, Cell, ReferenceLine, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import type { Balances } from '../../shared/api/types'
import { formatMoney } from '../../shared/format/money'

interface BalancesChartProps {
  members: Balances['members']
  meId: number
}

/** One horizontal bar per member: right and green when owed, left and orange when owing. */
export function BalancesChart({ members, meId }: BalancesChartProps) {
  const data = members.map(({ user, net }) => ({
    name: user.id === meId ? `${user.name} (you)` : user.name,
    net,
  }))

  return (
    <div style={{ width: '100%', height: 48 * data.length + 40 }} aria-label="Balances chart">
      <ResponsiveContainer>
        <BarChart data={data} layout="vertical" margin={{ top: 4, right: 16, bottom: 4, left: 4 }}>
          <XAxis
            type="number"
            tickFormatter={(value: number) => formatMoney(value)}
            tick={{ fontSize: 12, fill: 'var(--muted)' }}
            stroke="var(--line)"
          />
          <YAxis
            type="category"
            dataKey="name"
            width={84}
            tick={{ fontSize: 13, fill: 'var(--ink)' }}
            stroke="var(--line)"
          />
          <ReferenceLine x={0} stroke="var(--muted)" />
          <Tooltip
            cursor={{ fill: 'var(--bg)' }}
            // The bar's name is already the tooltip's heading, so show just "owes $553.20".
            separator=""
            formatter={(value) => {
              const net = Number(value)
              return [net < 0 ? `owes ${formatMoney(-net)}` : `is owed ${formatMoney(net)}`, '']
            }}
          />
          <Bar dataKey="net" radius={4} isAnimationActive={false}>
            {data.map((entry) => (
              <Cell key={entry.name} fill={entry.net < 0 ? 'var(--neg)' : 'var(--pos)'} />
            ))}
          </Bar>
        </BarChart>
      </ResponsiveContainer>
    </div>
  )
}

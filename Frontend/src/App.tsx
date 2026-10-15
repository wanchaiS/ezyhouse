import { Navigate, Outlet, Route, Routes } from 'react-router'
import { LoginPage } from './features/auth/LoginPage'
import { BillsPage } from './features/bills/BillsPage'
import { BondPage } from './features/bond/BondPage'
import { DashboardPage } from './features/dashboard/DashboardPage'
import { FurniturePage } from './features/furniture/FurniturePage'
import { GroceriesPage } from './features/groceries/GroceriesPage'
import { HouseholdPage } from './features/household/HouseholdPage'
import { RentPage } from './features/rent/RentPage'
import { SetupPage } from './features/setup/SetupPage'
import { useHousehold } from './shared/api/household'
import { useMe } from './shared/api/me'
import { ErrorMessage } from './shared/components/ErrorMessage'
import { Layout } from './shared/components/Layout'
import { Spinner } from './shared/components/Spinner'

/** /login and /register: only while signed out. */
function SignedOutOnly() {
  const me = useMe()
  if (me.isPending) return <Spinner />
  if (me.isError) return <ErrorMessage error={me.error} />
  return me.data ? <Navigate to="/" replace /> : <Outlet />
}

/** /setup: signed in, not in a household yet. */
function SetupOnly() {
  const me = useMe()
  if (me.isPending) return <Spinner />
  if (me.isError) return <ErrorMessage error={me.error} />
  if (!me.data) return <Navigate to="/login" replace />
  if (me.data.householdId !== null) return <Navigate to="/" replace />
  return <Outlet />
}

/** Every other screen: signed in and in a household, inside the Layout. */
function InHousehold() {
  const me = useMe()
  if (me.isPending) return <Spinner />
  if (me.isError) return <ErrorMessage error={me.error} />
  if (!me.data) return <Navigate to="/login" replace />
  if (me.data.householdId === null) return <Navigate to="/setup" replace />
  return <HouseholdLayout />
}

function HouseholdLayout() {
  const household = useHousehold()
  return (
    <Layout householdName={household.data?.name ?? ''}>
      <Outlet />
    </Layout>
  )
}

export default function App() {
  return (
    <Routes>
      <Route element={<SignedOutOnly />}>
        <Route path="/login" element={<LoginPage mode="login" />} />
        <Route path="/register" element={<LoginPage mode="register" />} />
      </Route>
      <Route element={<SetupOnly />}>
        <Route path="/setup" element={<SetupPage />} />
      </Route>
      <Route element={<InHousehold />}>
        <Route path="/" element={<DashboardPage />} />
        <Route path="/groceries" element={<GroceriesPage />} />
        <Route path="/bills" element={<BillsPage />} />
        <Route path="/rent" element={<RentPage />} />
        <Route path="/furniture" element={<FurniturePage />} />
        <Route path="/bond" element={<BondPage />} />
        <Route path="/household" element={<HouseholdPage />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}

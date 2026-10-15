import type { ReactNode } from 'react'
import { Link, NavLink } from 'react-router'
import { BillIcon, CartIcon, HomeIcon, KeyIcon, PeopleIcon, ShieldIcon, SofaIcon } from './icons'
import styles from './Layout.module.css'

const TABS = [
  { to: '/', label: 'Home', icon: <HomeIcon /> },
  { to: '/groceries', label: 'Groceries', icon: <CartIcon /> },
  { to: '/bills', label: 'Bills', icon: <BillIcon /> },
  { to: '/rent', label: 'Rent', icon: <KeyIcon /> },
  { to: '/furniture', label: 'Furniture', icon: <SofaIcon /> },
]

const PAGES = [
  { to: '/', label: 'Dashboard', icon: <HomeIcon /> },
  ...TABS.slice(1),
  { to: '/bond', label: 'Bond', icon: <ShieldIcon /> },
  { to: '/household', label: 'Household', icon: <PeopleIcon /> },
]

function classFor(base: string) {
  return ({ isActive }: { isActive: boolean }) => (isActive ? `${base} ${styles.active}` : base)
}

interface LayoutProps {
  householdName: string
  children: ReactNode
}

/** The signed-in frame: header, then a bottom tab bar below 820px or a sidebar from 820px. */
export function Layout({ householdName, children }: LayoutProps) {
  return (
    <div className={styles.shell}>
      <header className={styles.header}>
        <span className={styles.householdName}>{householdName}</span>
        <Link className={styles.iconBtn} to="/household" aria-label="Household">
          <PeopleIcon />
        </Link>
      </header>
      <div className={styles.body}>
        <nav className={styles.sidebar} aria-label="Pages">
          {PAGES.map((page) => (
            <NavLink key={page.to} to={page.to} end className={classFor(styles.navLink)}>
              {page.icon}
              <span>{page.label}</span>
            </NavLink>
          ))}
        </nav>
        <main className={styles.main}>{children}</main>
      </div>
      <nav className={styles.tabbar} aria-label="Tabs">
        {TABS.map((tab) => (
          <NavLink key={tab.to} to={tab.to} end className={classFor(styles.tab)}>
            {tab.icon}
            <span>{tab.label}</span>
          </NavLink>
        ))}
      </nav>
    </div>
  )
}

import React from 'react'
import { Outlet } from 'react-router-dom';

type Props = {
  children: React.ReactNode; // Da bi Tile.tsx moglo da se prosledi
  ticker: string;
}

// Zbog ovoga ce u CompanyPage.tsx da imamo <CompanyDashboard ticker={ticker!}> <Tile...> ...</CompanyDashboard>, a ticker! mora jer nije ticker: string | undefined u Props
// Moram imati built-in <Outlet /> koji omogucava nested routes tj children (Tile.tswx) i zato ne prosledjujem Tile kao Prop !
const Dashboard = ({children, ticker}: Props) => {
  return (
    <div className="relative md:ml-64 bg-blueGray-100 w-full">
      <div className="relative pt-20 pb-32 bg-lightBlue-500">
        <div className="px-4 md:px-6 mx-auto w-full">
          <div>
            <div className="flex flex-wrap">{children}</div> {/* children je Tile.tsx */}
            <div className="flex flex-wrap">{<Outlet context={ticker}/>}</div> {/* Mora <Outlet> zbog children: React.ReactNode + u Routes.tsx je definsano children za  */}
          </div>
        </div>
      </div>
    </div>
  )
}

export default Dashboard;
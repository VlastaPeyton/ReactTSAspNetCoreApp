import React, { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { CompanyProfile } from '../../company';
import { getCompanyProfile } from '../../Axios/api';
import Sidebar from '../../Components/Sidebar/Sidebar';
import CompanyDashboard from '../../Components/CompanyDashboard/CompanyDashboard';
import Tile from '../../Components/Tile/Tile';
import Spinner from '../../Components/Spinner/Spinner';
import TenK from '../../Components/TenK/TenK';

type Props = {}

/* Ovde smo stigli kada u SearchPage.tsx u input formi sam ukucao npr "tsla" ticker i prikazala se Tesla,Inc (TSLA) | USD | ... Card i kliknuo sam 
na "Tesla,Inc (TSLA)" i odvelo me je ovde, jer je u Routes definisana http://localhost:3000/company/:ticker za element=<CompanyPage /> 

  Sve sto je prikazano na CompanyPage pojavice se i na child pages of CompanyPage tj na CompanyProfile/IncomeStatement/BalanceSheet/Cashflow */
const CompanyPage = (props: Props) => {

  let {ticker} = useParams();  // u Routes.tsx, company route je "http://localhost:3000/company/:ticker", pa da se uzme sve posle : tj ticker uzima from URL 
  
  const [company, setCompany] = useState<CompanyProfile>();
  
  useEffect(() => {
    // Dobra praksa je staviti ovu metodu izvan useEffect 
    const getProfileInit = async () => {
      const result = await getCompanyProfile(ticker!); // Mora ! jer inace compile error posto ovime garantujem da ticker nece biti prazan string
      setCompany(result?.data[0]); // Mora ? jer u getCompanyProfile catch statement has no return i ako bude catch odradjen, bice undefined kao implicit return
    } 
    getProfileInit();
  }, []) // Samo tokom page loading ce ovo da se izvrsi, jer nema dependencies

  return (
    /* Mora <CompanyDashboard ticker={ticker!}> <Tile...>...</CompanyDashboard> jer CompanyDashboard.tsx ima children: React.ReactNode zbog Tile.tsx
    i zato ne prosledujem Tile kao Prop zbog built-in Outlet u CompanyDashboard ! Prosledio sam ticker!, a ne ticker, jer u CompanyDashboard Props NE stoji ticker: string | undefined
     */
    <>
    {company ? (
                <div className="w-full relative flex ct-docs-disable-sidebar-content overflow-x-hidden">
                  <Sidebar />
                  <CompanyDashboard ticker={ticker!}> 
                    <Tile title="Company Name" subTitle={company.companyName} />
                    <Tile title="Price" subTitle={'$' + company.price} /> {/* price je number -> Tile Props subTitle mora biti string | number  */}
                    <Tile title="Sector" subTitle={company.sector} />
                    <Tile title="DCF" subTitle={'$' + company.dcf.toString()} /> {/* Moglo i kao number jer u Tile namesteno da prima string ili number */}
                    <TenK ticker={company.symbol} />
                    <p className="bg-white shadow rounded text-medium text-gray-900 p-3 mt-1 m-4">{company.description}</p>
                  </CompanyDashboard>
                </div> ) 
                
             : (<Spinner />)}
    </>
  )
}

export default CompanyPage;
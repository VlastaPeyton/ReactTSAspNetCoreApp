import { useEffect, useState } from 'react'
import { CompanyKeyMetrics } from '../../company';
import { useOutletContext } from 'react-router-dom';
import { getKeyMetrics } from '../../Axios/api';
import RatioList from '../RatioList/RatioList';
import Spinner from '../Spinner/Spinner';
import { formatLargeNonMonetaryNumber, formatRatio } from '../../NumberFormatting/NumberFormatting';
import StockComment from '../StockComment/StockComment';

type Props = {}

// Vrste i vrednosti njihove koje ce se pojaviti na http://localhost:3000/company/:ticker/company-profile tj kad kliknem Company-profile u Sidebar.tsx
const tableConfig = [
  {
    label: "Market Cap",
    render: (company: CompanyKeyMetrics) => formatLargeNonMonetaryNumber(company.marketCapTTM),
    subTitle: "Ukupna vrednost svih firminih stocks"
  },
  {
    label: "Current Ratio",
    render: (company: CompanyKeyMetrics) => formatRatio(company.currentRatioTTM),
    subTitle: "Firmina sposobnost da plati kratkorocna dugovanja"
  },
  {
    label: "Return On Equity",
    render: (company: CompanyKeyMetrics) => formatRatio(company.roeTTM),
    subTitle: "Mera firminog net income podeljen sa shareholders equity"
  },
  {
    label: "Return On Asset",
    render: (company: CompanyKeyMetrics) => formatRatio(company.cashPerShareTTM),
    subTitle: "Koliko firma efektno koristi svoje assets"
  },
];

const CompanyProfile = (props: Props) => {
  const ticker = useOutletContext<string>(); // Get data from parent route(pogledaj Routes.tsx) (http://localhost:3000/company/:ticker) tj ticker uzima (npr "TSLA")
  const [companyData, setCompanyData] = useState<CompanyKeyMetrics>();
  
  useEffect(() => {
    const getCompanyMetrics = async () => {
      const result = await getKeyMetrics(ticker); 
      // result je niz of CompanyKeyMetrics elements  i zato RatioLists moze da renderuje sve jer polja iz result niza (tj jednog elementa,jer RatioList radi sa 1 elem) postoje u tableConfig render funkcijama
      setCompanyData(result?.data[0]); // Jer getKeyMetrcis from api.tsx moze da vrati i null, pa zato ? mora 
      // companyData je definisan kao jedan element of CompanyKeyMetrics tipa, pa mora result?.data[i], plus RatioList je definisan bas da obradi samo jedan element 
    }
    getCompanyMetrics();
  },[]); 

  return (
    <>
    {companyData ? (<> <RatioList data={companyData} config={tableConfig} />
                       <StockComment stockSymbol={ticker} />
                    </> ) 
                    
                  : (<Spinner />)}
    </>
  )
}

export default CompanyProfile;
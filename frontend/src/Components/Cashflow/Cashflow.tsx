import React, { useEffect, useState } from 'react'
import { CompanyCashFlow } from '../../company';
import { useOutletContext } from 'react-router-dom';
import { getCashflow } from '../../Axios/api';
import Table from '../Table/Table';
import Spinner from '../Spinner/Spinner';
import { formatLargeMonetaryNumber } from '../../NumberFormatting/NumberFormatting';

type Props = {}

// Kolone tabele koja ce biti prikaza na http://localhost:3000/company/:ticker/cashflow tj kad klikenm u Cashflow u Sidebar.tsx
const tableConfig = [
  {
    label: "Date",
    render: (company: CompanyCashFlow) => company.date,
  },
  {
    label: "Operating Cashflow",
    render: (company: CompanyCashFlow) => formatLargeMonetaryNumber(company.operatingCashFlow),
  },
  {
    label: "Investing Cashflow",
    render: (company: CompanyCashFlow) => formatLargeMonetaryNumber(company.netCashUsedForInvestingActivites),
  },
  {
    label: "Financing Cashflow",
    render: (company: CompanyCashFlow) => formatLargeMonetaryNumber(company.netCashUsedProvidedByFinancingActivities),
  },
  {
    label: "Cash At End of Period",
    render: (company: CompanyCashFlow) => formatLargeMonetaryNumber(company.cashAtEndOfPeriod),
  },
  {
    label: "CapEX",
    render: (company: CompanyCashFlow) => formatLargeMonetaryNumber(company.capitalExpenditure),
  },
  {
    label: "Issuance Of Stock",
    render: (company: CompanyCashFlow) => formatLargeMonetaryNumber(company.commonStockIssued),
  },
  {
    label: "Free Cash Flow",
    render: (company: CompanyCashFlow) => formatLargeMonetaryNumber(company.freeCashFlow),
  },
];

function Cashflow({}: Props) {
  const ticker = useOutletContext<string>(); // Get data from parent route(pogledaj Routes.tsx) (http://localhost:3000/company/:ticker) tj ticker uzima (npr "TSLA")
  const [cashflow, setCashflow] = useState<CompanyCashFlow[]>();

  useEffect(() => {
    const fetchCashflow = async () => {
      const result = await getCashflow(ticker);
      // result je niz od CompanyCashFlow elemenata i zato Table moze da renderuje sve, jer polja iz result niza postoje u tableConfig render funkcijama
      setCashflow(result?.data); // Jer getCashflow from api.tsx moze da vrati i null, pa zato ? mora 
      // cashflow je definisan kao niz of CompanyCashFlow elemenata i zato result?.data, plus Table definisan da obradi niz
    }

    fetchCashflow();
  },[])

  return (
    <>
      {cashflow ? (<Table  data={cashflow} config={tableConfig} />) : (<Spinner />)}
    </>
  )
}

export default Cashflow;
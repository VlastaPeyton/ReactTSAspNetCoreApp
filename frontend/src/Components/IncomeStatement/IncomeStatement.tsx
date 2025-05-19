import React, { useEffect, useState } from 'react'
import { CompanyIncomeStatement } from '../../company';
import { useOutletContext } from 'react-router-dom';
import { getIncomeStatement } from '../../Axios/api';
import Table from '../Table/Table';
import Spinner from '../Spinner/Spinner';
import { formatLargeMonetaryNumber, formatRatio } from '../../NumberFormatting/NumberFormatting';

type Props = {}

// Kolone tabele koja ce biti prikaza na http://localhost:3000/company/:ticker/income-statement tj kad klikenm u Income-statement u Sidebar.tsx
const tableConfig = [
  {
    label: "Date",
    render: (company: CompanyIncomeStatement) => company.date,
  },
  {
    label: "Revenue",
    render: (company: CompanyIncomeStatement) => formatLargeMonetaryNumber(company.revenue),
  },
  {
    label: "Cost Of Revenue",
    render: (company: CompanyIncomeStatement) => formatLargeMonetaryNumber(company.costOfRevenue),
  },
  {
    label: "Depreciation",
    render: (company: CompanyIncomeStatement) => formatLargeMonetaryNumber(company.depreciationAndAmortization),
  },
  {
    label: "Operating Income",
    render: (company: CompanyIncomeStatement) => formatLargeMonetaryNumber(company.operatingIncome),
  },
  {
    label: "Income Before Taxes",
    render: (company: CompanyIncomeStatement) => formatLargeMonetaryNumber(company.incomeBeforeTax),
  },
  {
    label: "Net Income",
    render: (company: CompanyIncomeStatement) => formatLargeMonetaryNumber(company.netIncome),
  },
  {
    label: "Net Income Ratio",
    render: (company: CompanyIncomeStatement) => formatRatio(company.netIncomeRatio),
  },
  {
    label: "Earnings Per Share",
    render: (company: CompanyIncomeStatement) => formatRatio(company.eps),
  },
  {
    label: "Earnings Per Diluted",
    render: (company: CompanyIncomeStatement) => formatRatio(company.epsdiluted),
  },
  {
    label: "Gross Profit Ratio",
    render: (company: CompanyIncomeStatement) => formatRatio(company.grossProfitRatio),
  },
  {
    label: "Opearting Income Ratio",
    render: (company: CompanyIncomeStatement) => formatRatio(company.operatingIncomeRatio),
  },
  {
    label: "Income Before Taxes Ratio",
    render: (company: CompanyIncomeStatement) => formatRatio(company.incomeBeforeTaxRatio),
  },
];

const IncomeStatement = (props: Props) => {
  const ticker = useOutletContext<string>(); // Get data from parent route(pogledaj Routes.tsx) (http://localhost:3000/company/:ticker) tj ticker uzima (npr "TSLA")
  const [incomeStatement, setIncomeStatement] = useState<CompanyIncomeStatement[]>();

  useEffect(() => {
    const fetchIncomeStatement = async() => {
      const result = await getIncomeStatement(ticker); 
      // result je niz od CompanyIncomeStatement elemenata i zato Table moze da renderuje sve, jer polja iz result niza postoje u tableConfig render funkcijama
      setIncomeStatement(result?.data); // Jer getIncomeStatement from api.tsx moze da vrati i null, pa zato ? mora 
      // incomeStatement je definisano kao niz of CompanyIncomeStatement elemenata i zato result?.data, plus Table definisan da obradi niz
    }

    fetchIncomeStatement();

  },[]);

  return (
    <>
      {incomeStatement ? (<Table  data={incomeStatement} config={tableConfig} />) : (<Spinner />)}
    </>
  )
}

export default IncomeStatement;
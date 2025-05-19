import React, { useEffect, useState } from 'react'
import { CompanyBalanceSheet } from '../../company';
import { useOutletContext } from 'react-router-dom';
import { getBalanceSheet } from '../../Axios/api';
import RatioList from '../RatioList/RatioList';
import Spinner from '../Spinner/Spinner';
import { formatLargeMonetaryNumber } from '../../NumberFormatting/NumberFormatting';

type Props = {}

// Kolone tabele koja ce biti prikazana na http://localhost:3000/company/:ticker/balance-sheet tj kad klikenm u Balance-sheet u Sidebar.tsx
const tableConfig = [
    {
      label: "Total Assets",
      render: (company: CompanyBalanceSheet) => formatLargeMonetaryNumber(company.totalAssets),
    },
    {
      label: "Current Assets",
      render: (company: CompanyBalanceSheet) => formatLargeMonetaryNumber(company.totalCurrentAssets),
    },
    {
      label: "Total Cash",
      render: (company: CompanyBalanceSheet) => formatLargeMonetaryNumber(company.cashAndCashEquivalents),
    },
    {
      label: "Property & equipment",
      render: (company: CompanyBalanceSheet) => formatLargeMonetaryNumber(company.propertyPlantEquipmentNet),
    },
    {
      label: "Intangible Assets",
      render: (company: CompanyBalanceSheet) => formatLargeMonetaryNumber(company.intangibleAssets),
    },
    {
      label: "Long Term Debt",
      render: (company: CompanyBalanceSheet) => formatLargeMonetaryNumber(company.longTermDebt),
    },
    {
      label: "Total Debt",
      render: (company: CompanyBalanceSheet) => formatLargeMonetaryNumber(company.otherCurrentLiabilities),
    },
    {
      label: "Total Liabilites",
      render: (company: CompanyBalanceSheet) => formatLargeMonetaryNumber(company.totalLiabilities),
    },
    {
      label: "Current Liabilities",
      render: (company: CompanyBalanceSheet) => formatLargeMonetaryNumber(company.totalCurrentLiabilities),
    },
    {
      label: "Long-Term Debt",
      render: (company: CompanyBalanceSheet) => formatLargeMonetaryNumber(company.longTermDebt),
    },
    {
      label: "Long-Term Income Taxes",
      render: (company: CompanyBalanceSheet) => formatLargeMonetaryNumber(company.otherLiabilities),
    },
    {
      label: "Stakeholder's Equity",
      render: (company: CompanyBalanceSheet) => formatLargeMonetaryNumber(company.totalStockholdersEquity),
    },
    {
      label: "Retained Earnings",
      render: (company: CompanyBalanceSheet) => formatLargeMonetaryNumber(company.retainedEarnings),
    },
  ];
  
const BalanceSheet = (props: Props) => {
    const ticker = useOutletContext<string>(); // Get data from parent route(pogledaj Routes.tsx) (http://localhost:3000/company/:ticker) tj ticker uzima (npr "TSLA")
    const [balanceSheet, setBalanceSheet] = useState<CompanyBalanceSheet>();
    
    useEffect(() => {
        const fetchBalanceSheet = async () => {
            const result = await getBalanceSheet(ticker);
            // result je niz of CompanyBalanceSheet elemenata i zato RatioLists moze da renderuje sve jer polja iz result niza (tj jednog elementa,jer RatioList radi sa 1 elem) postoje u tableConfig render funkcijama
            setBalanceSheet(result?.data[0]); // Jer getBalanceSheet from api.tsx moze da vrati i null, pa zato ? mora 
            // balanceSheet je definisan kao jedan element CompanyBalanceSheet tipa, pa mora result?.data[i], plus RatioList je definisan bas da obradi samo jedan element 
        }

        fetchBalanceSheet();
    },[])

    return (
        <>
            {balanceSheet ? (<RatioList data={balanceSheet} config={tableConfig} /> ) : (<Spinner />)}
        </>
    )
}

export default BalanceSheet;
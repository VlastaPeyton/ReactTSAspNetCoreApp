import React, { useEffect, useState } from 'react'
import { CompanyTenK } from '../../company';
import { getTenK } from '../../Axios/api';
import TenKItem from './TenKItem';
import Spinner from '../Spinner/Spinner';

type Props = {
    ticker: string;
}

const TenK = ({ticker}: Props) => {
    const [companyData, setCompanyData] = useState<CompanyTenK[]>();
    useEffect(() => {
        const fetchTenK = async () => {
            const result = await getTenK(ticker);
            // result je niz od CompanyTenK elemenata i zato Table moze da renderuje sve, jer polja iz result niza postoje u tableConfig render funkcijama
            setCompanyData(result?.data); // getTenK from api.tsx moze da vrati i null, pa zato ? mora 
            // companyData je definisano kao niz of CompanyTenK elemenata i zato result?.data, plus Table definisan da obradi niz
        }
        fetchTenK();

    }, [ticker]);

    return (
        <div className="inline-flex rounded-md shadow-sm m-4">
            {companyData ? (companyData.slice(0, 5).map((tenK, index) => {return <TenKItem key={index} tenK={tenK}/> })) : (<Spinner />)} 
        </div>
    )
    // Mora key, iako TenKItem neka key, jer .map zahteva to da bi HTML znao da prati elemente liste. Stavio index, jer CompanyTenK type nema nista unique field
}

export default TenK;
import React from 'react'
import Table from '../../Components/Table/Table';
import RatioList from '../../Components/RatioList/RatioList';
import { CompanyKeyMetrics } from '../../company';
import { testIncomeStatementData } from '../../Components/Table/testData';
//testIncomeStatementData samo za "AAPL" imam i zato ovde ne ocitavam preko api.tsx

type Props = {}

// Tabela, sa 1 kolonom, koja ce se pojaviti u http://localhost:3000/design-guide 
const tableConfig = [
  { 
    label: "Market Cap",
    render: (company: CompanyKeyMetrics) => company.marketCapTTM, 
    subTitle: "Total value of all"
  },
];
/* Pogledaj koja polja ima element of testIncomeStatementData -> ni jedno polje u testIncomeStatementData nije isto kao u CompanyKeyMetrics i to pravi error u RatioList.tsx/Table.tsx kod row.render(data) 
da se ne moze prikaze nista kroz RatioList, ali u RatioList.tsx u Props je data:any i zato nema error, ali svakako ne prikaze nista.

  RatioList ocekuje samo 1 element of testIncomeStatementData i zato je tako kodiran => mora testIncomeStatementData[0] 
  Table ocekuje ceo testIncomeStatementData niz i zato je tako kodiran => mora testIncomeStatementData  */

/* U Routes.tsx definisano http://localhost:3000/design-guide za ovu stranicu. */
const DesignGuide = (props: Props) => {
  return (
    <>
        <h1>Desing guide</h1>
        <RatioList data={testIncomeStatementData[0]} config={tableConfig} />
        <Table data={testIncomeStatementData} config={tableConfig} />

    </>
  )
}

export default DesignGuide;
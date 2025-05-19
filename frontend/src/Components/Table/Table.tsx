// Koristi u DesingGuide.tsx / IncomeStatement.tsx / Cashflow.tsx, pa je reusable component

type Props = {
    config: any; // tableConfig from DesignGuide/ IncomeStatement / Cashflow
    data: any; 
    // Isto objasnjenje kao u RatioList.tsx
}

// Modifikuj sa generic kodom bas kao u RatioList sto je uradjeno da ne bude row:any i config:any 

const Table = ({data, config}: Props) => {
    // Ocekuje data kao listu i zato ovako je napisano sa data.map i config.map 

    /* .map pravi listu, a React mora da prati elemente liste (vrste tabele) i zato svaki mora imati unique key. 
    <tr> imace key={index} jer nzm sta u elementu of testIncomeStatementData je unique a mrzi me da trazim 
    <td> ne valja da index jer <tr> ima to, ali moze {val.label} jer in both DesignGuide/IncomeStatement tableConfig ima label polje koje je unique 
    <th> imace key={config.label} jer to je unique, pa ne mora index , a <th> se odnosi na config a tu znam da je svaki label unique (vidim u DesignGuide)
    
       data.map((row...) row je svaki element iz testIncomeStatementData niza (DesignGuide) / CompanyIncomeStatement type niza (IncomeStatement) / CompanyCashFlow type niza (Cashflow). 
    Zbog row:any, VSC dozvoljava da bilo sta ukucamo, i zato treba se Table odraditi kao generic u RatioList sto pise, jer row.nepostojecePolje dovodi do greske koju jedva nadjem. */
    const renderedRows = data.map((row: any, index: number) => {
        // Inside same list (same .map) , every thing has to have different unique key 
        return (                
            <tr key={index}> 
                {config.map((val: any) => {
                    return (<td className="p-4 whitespace-nowrap text-sm font-normal text-gray-900" key={val.label}>{val.render(row)}</td>)
                })}
            </tr>
        )
    })

    const renderedHeaders = config.map((config: any) => {
        // Ovde moze key isti kao <td> jer <td> je u razlicitoj listi (different .map) 
        return (                
            <th className='p-4 text-left text-xs font-medium text-gray-500 uppercase tracking-wider' key={config.label}>{config.label}</th>
        )
    })

    return (
        <div className='bg-white shadow rounded-lg p-4 sm:p-6 xl:p-8'>
            <table>
                <thead className="min-w-full divide-y divide-gray-200 m-5">
                    <tr>{renderedHeaders}</tr>
                </thead>
                <tbody>{renderedRows}</tbody>
            </table>
        </div>
  )
}

export default Table;
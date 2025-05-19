
/* Da bi ovo bio pravi TS Reusable Component, mora :
    export type RowConfig<T> ={
        label: string;     // Jer tableConfig in both DesignGuide and CompanyProfile ima ovo polje
        subTitle?: string; // Jer tableConfig u DesignGudie ima, CompanyProfile nema ovo polje
        render: (item: T) => React.ReactNode; 
        };
        
    type Props<T> ={
        data: T[]; // Jer sam u DesignGuide.tsx prosledio testIncomeStatementData tj niz a ne samo jedan element
        config: RowConfig<T>[];
    };

    function RatioList<T>({data, config}: Props<T>){    // ili const RatioList<T,>
            const renderedRows = config.map((row, index) => {
                return (....)
            })
            return (...)
    }

    U DesignGuide/CompanyProfile/BalanceSheet onda mora <RatioList<CompanyKeyMetrics> data... />   
*/

type Props = {
    data: any;   
    config: any; // tableConfig from DesignGuide/ CompanyProfile / BalanceSheet
    // any, jer reusable component posto koristim RatioList in DesignGuide.tsx /CompanyProfile.tsx / BalanceSheet.tsx
}  

// Koristi se u DesignGuide.tsx, CompanyProfile.tsx i BalanceSheet.tsx, jer ovo je prikaz svake vrste 
const RatioList = ({data, config }: Props) => {
    /*.map pravi listu, pa u <li> moram key unique imati da bi React pratio elemente liste, a to moze recimo biti index ili neko polje sa unique value iz testIncomeStatementData niza (DesignGuide)
    ili CompanyKeyMetric elementa (CompanyProfile) ili CompanyBalanceSheet elementa (BalanceSheet). Zbog config.map((row:any...) row je svako polje iz tableConfig, VSC dozvoljava bilo sta da kucam i onda moze row.nepostojecePolje 
    i onda dodje do greske ako to polje ne postoji u tableConfig u DesignGuide/CompanyProfile/BalanceSheet. Zato, ovo moram napraviti kao iznad sto pise generic da ne bi
    doslo do problema ovog potencijalnog da ne omasim i ukucam polje koje ne postoji tj zelim da samo moze row.label, row.subTitle i row.rende.
    */

    // Ovo ce samo da napravi, u DesignGuide.tsx/CompanyProfile/BalanceSheet, jedno polje, iznad tabele, tipa "Company Name" | "This is company name" |  "Apple"
    const renderedRows = config.map((row:any, index:number) => {
        return (         // Nije dobro row:any i zato vidi da popravis to sa generic u props ali u tom slucaju RatioList mora biti function generic a ne const generic
            <li className="py-3 sm:py-4" key={index}>
                <div className="flex items space">
                    <div className='flex-1 min-w-0'>
                        <p className="text-sm fontmt-medium text-gray-900 truncate">{row.label}</p> {/* Postoji label u tableConfig in both CompanyProfile and DesignGuide, pa ne mora if else  */}
                        <p className="text-sm text-gray-500 truncate">{row.subTitle && row.subTitle}</p> {/* U BalanceSheet, tableConfig nema subtitle i zato ovako if else mora */}
                    </div>
                    <div className="inline-flex items-center text-base font-semibold text-gray-900">{row.render(data)}</div> {/* Postoji render u tableConfing in both CompanyProfile and DesignGuide, pa ne mora if else */}
                </div>
            </li>
        )
    })

    return (
        <div className='bg-white shadow rounded-lg ml-4 mt-4 mb-4 p-4 sm:p-6 h-full'>
            <ul className='divide-y divided-gray-200'>{renderedRows}</ul> {/* Jer svaki row ima <li>, pa mora <ul> za ceo renderedRows */}
        </div>
    )
}

export default RatioList;
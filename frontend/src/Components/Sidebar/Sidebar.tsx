import exp from 'constants'
import React from 'react'
import { Link } from 'react-router-dom'
import {FaHome, FaTable, FaMoneyBill } from "react-icons/fa"; 
import {FaTableCells} from "react-icons/fa6";

type Props = {}

// Compoent for CompanyPage.tsx (http://localhost:3000/company/:ticker)
const Sidebar = (props: Props) => {
    const HomeIcon = FaHome as unknown as React.FC; // Mora ovo, jer <FaHome /> nece u TS
    const IncomeIcon = FaMoneyBill as unknown as React.FC;
    const TableIcon = FaTable as unknown as React.FC;
    const TableCellIcon = FaTableCells as unknown as React.FC;

    /* Zbog Routes.tsx, sve iz CompanyPage (CompanyDashBoard i Sidebar, bice prikazano i na company/:ticker/company-profile i company/:ticker/income-statement)  */
    return (
        <nav className="block py-4 px-6 top-0 bottom-0 w-64 bg-white shadow-xl left-0 absolute flex-row flex-nowrap md:z-10 z-9999 transition-all duration-300 ease-in-out transform md:translate-x-0 -translate-x-full">
            <button className="md:hidden flex items-center justify-center cursor-pointer text-blueGray-700 w-6 h-10 border-l-0 border-r border-t border-b border-solid border-blueGray-100 text-xl leading-none bg-white rounded-r border border-solid border-transparent absolute top-1/2 -right-24-px focus:outline-none z-9998">
                <i className="fas fa-ellipsis-v"></i>
            </button>

            <div className="flex-col min-h-full px-0 flex flex-wrap items-center justify-between w-full mx-auto overflow-y-auto overflow-x-hidden">
                <div className="flex bg-white flex-col items-stretch opacity-100 relative mt-4 overflow-y-auto overflow-x-hidden h-auto z-40 items-center flex-1 rounded w-full">
                    <div className="md:flex-col md:min-w-full flex flex-col list-none">
                        <Link to="company-profile" 
                            className="flex md:min-w-full text-blueGray-500 text-medium uppercase font-bold block pt--1 pb-4 no-underline">
                        <HomeIcon /> {/* Omoguceno u Routes.tsx a definisano u CompanyProfile.tsx da */}
                        <h6 className="ml-3">Company profile</h6>
                        </Link>
                        
                        <Link to="income-statement" 
                            className="flex md:min-w-full text-blueGray-500 text-medium uppercase font-bold block pt--1 pb-4 no-underline">
                        <TableIcon /> {/* Omoguceno u Routes.tsx a definisano u IncomeStatement.tsx */}
                        <h6 className='ml-3'>Income statement</h6>
                        </Link>

                        <Link to="balance-sheet"
                              className="flex md:min-w-full text-blueGray-500 text-medium uppercase font-bold block pt--1 pb-4 no-underline">
                        <IncomeIcon /> {/* Omoguceno u Routes.tsx a definisano u BalanceSheet.tsx */}
                        <h6 className="ml-3">Balance sheet</h6>
                        </Link>
                        
                        <Link to="cashflow"
                              className="flex md:min-w-full text-blueGray-500 text-medium uppercase font-bold block pt--1 pb-4 no-underline">
                        <TableCellIcon /> {/* Omoguceno u Routes.tsx a definisano u CashFlow.tsx */}
                        <h6 className="ml-3">Cashflow</h6>
                        </Link>

                    </div>
                </div>
            </div>
        </nav>
    )
}

export default Sidebar;
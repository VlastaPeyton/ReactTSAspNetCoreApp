import React from 'react'
import { CompanyTenK } from '../../company';
import { Link } from 'react-router-dom';

type Props = {
    tenK: CompanyTenK;
}

const TenKItem = ({tenK}: Props) => {
    const fillingDate = new Date(tenK.fillingDate).getFullYear();
    /* reloadDocument jer mi treba full page reload jer samo tako oce raditi. 
       reloadDocument sluzi najcesce za Login/Logout da se state variable izbrisu.
       
       Mora type="button" inace nece raditi 
       Link me vodi na finalLink koji ne znam kako izgleda, ali getTenK iz api.tsx ga vrati kroz objekat */
    return <Link to={tenK.finalLink} 
                 reloadDocument
                 type="button" 
                 className='inline-flex items-center p-4 text-md text-white bg-lightGreen'>
                    10K - {tenK.symbol} - {fillingDate} 
            </Link>
    
}

export default TenKItem;
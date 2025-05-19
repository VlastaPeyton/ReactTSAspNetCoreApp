import React, { SyntheticEvent } from 'react'
import Card from '../Card/Card';
import { CompanySearch } from '../../company';

type Props = {
  searchResults: CompanySearch[];
  onPortfolioCreate: (e: SyntheticEvent) => void;
}

// Lista svih companija koje imaju slican ticker 
const CardList = ({searchResults, onPortfolioCreate}: Props) => {
  /* Card ima id, searchResult, onPortfolioCreate, ali odavde prosledjujemo i key iako Card nema taj Prop, jer React to trazi in order to track lists items zbog .map, 
  a key mora biti unique, pa result.sumbol sam prosledio, jer je to unique, ali npr u slucaju ListPortfolio.tsx neam unique, pa koristim index. */
  return (
    <>
      {searchResults.length > 0 ? (searchResults.map((result) => {return <Card id={result.symbol} searchResult={result}  onPortfolioCreate={onPortfolioCreate} key={result.symbol}/>})) 
                            : (<p className="mb-3 mt-3 text-xl font-semibold text-center md:text-xl">No results yet</p>) } {/* Ako ukucam nepostojeci ticker ovo se prikaze */}
    </>
  )
};

export default CardList;
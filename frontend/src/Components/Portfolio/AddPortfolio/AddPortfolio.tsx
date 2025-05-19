import React, { SyntheticEvent } from 'react'

type Props = {
    onPortfolioCreate: (e: SyntheticEvent) => void;
    symbol: string;
}

const AddPortfolio = ({onPortfolioCreate, symbol}: Props) => {
    /* In <form>, onSubmit functions (onPortfolioCreate) mora imati e.preventDefault() in it's definition 
    dok kod onChange ili onClick (u <form> ili van forme), skoro nikad nema e.preventDefault().
       <input name="symbol"> da bi onPortfolioCreate znao da doda ovu vrednost u portfolioValues niz. */
    return (
        <form onSubmit={(e) => onPortfolioCreate(e)}> 
            <input name="symbol" readOnly={true} hidden={true} value={symbol} />
            <button type="submit">Add</button>
        </form>
    )
}

export default AddPortfolio;
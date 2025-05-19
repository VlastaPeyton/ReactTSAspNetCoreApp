import React, { SyntheticEvent, useEffect, useState } from 'react'
import { CompanySearch } from '../../company';
import { searchCompanies } from '../../Axios/api';
import Navbar from '../../Components/Navbar/Navbar';
import Hero from '../../Components/Hero/Hero';
import Search from '../../Components/Search/Search';
import ListPortfolio from '../../Components/Portfolio/ListPortfolio/ListPortfolio';
import CardList from '../../Components/CardList/CardList';
import { PortfolioGetFromBackend } from '../../Models/PortfolioGetFromBackend';
import { portfolioAddApi, portfolioDeleteApi, portfolioGetApi } from '../../Services/PortfolioService';
import { toast } from 'react-toastify';

type Props = {}

const SearchPage = (props: Props) => {
  /* Za proste tipove, moze kao u JS useState(defaultValue), jer ce iz defaultValue da svali koji je tip, ali za 
  Reference types, mora useState<tip>(defaultValue), stoga i za proste tipove cu pisati kao za Reference type jer je dobra praksa. */
  const [search, setSearch] = useState<string>(""); 
  
  const [searchResults, setSearchResults] = useState<CompanySearch[]>([])

  const [serverError, setServerError] = useState<string | null>(null); // Za gresku u axios koja moze biti string(ako bilo greske) ili null(ako nema greske)

  // Svaki put kad dodam Card u Portfolio, prilikom page loading ocu da mi stoji sta sam sve addovao do sad i kad ugasim app ovo ce da stoji 
  // const [portfolioValues, setPortfolioValues] = useState<string[]>(() => {
  //   const saved = localStorage.getItem("portfolioValuesLocalStorage");
  //   return saved ? JSON.parse(saved) : [];
  // }); -ovo je bilo kad sam Card ocitavao sa neta, a sad ocu iz Portfolios tabele
  const [portfolioValues, setPortfolioValues] = useState<PortfolioGetFromBackend[] | null>([]);
  
  // Save portfolioValues to localStorage whenever it changes kako bi se sacuvale sve Card(Portfolios) koje sam addovao i kad odem na neku drugu stranicu
  // useEffect(() => {
  //   localStorage.setItem("portfolioValuesLocalStorage", JSON.stringify(portfolioValues));
  // }, [portfolioValues]);
  // Ovo izmeni jer ocu iz baze da se ucita a ne localstorage
  useEffect(() => {
    getPortfoliosFromBackend();
  },[]);

  const getPortfoliosFromBackend = () => {
    portfolioGetApi().then((result)=>{
      if(result?.data){
        setPortfolioValues(result?.data);
      }
    }).catch((err)=>{
      toast.warning("Couldnt get portfolios");
    })
  }

  /* Ova metoda pokrece se iz Search.tsx preko onChange, a onChange ne trazi e.preventDefault() u funkciji koju referencira,
  stoga ovde nema e.preventDefault() i zato sam prosledio direktno e.target.value, a ne e(Event). */
  const handleSearchChange = (eTargetValue: string) : void => {
    setSearch(eTargetValue); // search dobije vrednost koju sam uneo kroz formu in Search.tsx 
  };

  /* Moze SyntheticEvent koji hvata vecinu Eventa razlicitog tipa, a mogo je i neki konkretni Event type biti.
    Ova metoda se pokrece iz Search.tsx preko <form onSubmit> i zato mora da ima e.preventDefault() kako browser
    ne bi reload ovu stranicu i da izgubim state variable serverError i searchresults, stoga argument je Event type. */
  const onSearchSubmit = async (e: SyntheticEvent) => {
    e.preventDefault(); 
    const result = await searchCompanies(search); // search ima vrednost dobijenu kroz formu in Search.tsx
    // api.tsx vraca response (not response.data) ako je dobro ili error ako nije dobro. Ako je dobro, ne moze ovde response.data bez prethodno provere da li je niz, jer je TS u pitanju! 
    if (typeof(result) == "string"){ // Da li je axios vratio gresku jer tad return je string tipa.
      setServerError(result);
    }
    else if (Array.isArray(result.data)){ // Da li je axios vratio podatke. Ne moze isArray(result), jer u searchCompanies je return response, a ne response.data u try block.
      setSearchResults(result.data); // Sada moze result.data, jer smo proverili tip return vrednosti iz searchCompanies of api.tsx
    }
  };

  /* Ova metoda se pokrece iz AddPortfolio.tsx (preko CardList.tsx) preko <form onSubmit> i zato mora da ima e.preventDefault() kako browser
  ne bi reload ovu stranicu i da izgubim state variables serverError i searchresults. 
    Iako u AddPortfolio.tsx, pise SyntheticEvent za tip argumenta, ovde mora "e: any" (ne moze "e: SyntheticEvent"), jer ovo su slucajevi kad moram
  da iskljucim TS kako bi moglo e.target[0].value.  */
  const onPortfolioCreate = (e: any) => {
    e.preventDefault(); // Prevent page reload when submitting a form in AddPortfolio.tsx

    // Ovo ispod markiram, jer vise ne citam sa sajta, vec iz baze 
    /*const formData = new FormData(e.target); // e.target je cela Forma iz AddPortfolio.tsx
    const symbol = formData.get("symbol");  // jer <input name="symbol"> u formi iz AddPortfolio.tsx 
    // Mora provera, jer symbol mozda bude i null, a ne moze da doda null to postojeci portfolioValues jer useState<string[]> a nije useState<string[] | null>  
    if (typeof(symbol) === "string"){
      const updatedPorfolio = [...portfolioValues, symbol]; // Dodat novi portfolio u postojeci portfolioValues
      setPortfolioValues(updatedPorfolio); // Sada ListPortfolio nije prazan i prikazace se companija koju sam dodao
    }
    */

    portfolioAddApi(e.target[0].value).then((res) => {
      if(res?.status === 204){
        toast.success("Stock added to portfolio");
        getPortfoliosFromBackend();
      }
    }).catch((err)=>{
      toast.warning("Couldnt create portfolio");
    });
  }; 
    
  // const onPortfolioDelete = (indexToDelete: number) => {
  //   // filter pravi listu kao i map 
  //   const updated = portfolioValues.filter((_, index) => index !== indexToDelete);
  //   setPortfolioValues(updated);
  // }; -ovo koristio sam dok sam sa neta imao portfolio a sad cu iz baze

  const onPortfolioDelete = (symbol: string) => {
    portfolioDeleteApi(symbol).then((result) => {
      if(result?.status === 200){
        toast.success("Stock deleted from portfolios");
        getPortfoliosFromBackend(); // Da ocita opet listu gde ima jedan manje nakon 
      }
    });
  }


  return (
    <div className="App">
      <Hero />
      <Search onSearchSubmit={onSearchSubmit} search={search} handleSearchChange={handleSearchChange} />
      {serverError && <h1>{serverError}</h1>}  {/* If serverError not null, then show "Network Error" */}
      <ListPortfolio portfolioValues={portfolioValues!} onPortfolioDelete={onPortfolioDelete}/> 
      <CardList searchResults={searchResults} onPortfolioCreate={onPortfolioCreate} /> {/* Kad ukucam ticker u search form, prvo se CardList aktivira, pa tek ListPortfolio se promeni ako dodam ticker Card kroz AddPortfolio. */}
    </div>
  )
  
}


export default SearchPage;

import { ClipLoader } from 'react-spinners';
import "./Spinner.css";

type Props = {
    isLoading?: boolean; // ? jer je optional Prop
}

// isLoading je optional, pa ako ga ne prosledim, zelim npr da bude true (inace je undefined ako nema default value)
const Spinner = ({isLoading = true}: Props) => {
  return (
    <div id="loading-spinner">
        <ClipLoader color="#36d7b7" loading={isLoading} size={35} aria-label='Loading Spinner' />
    </div>
  )
}

// U Spinner.css namestim da Spinner bude u centru stranice
export default Spinner;
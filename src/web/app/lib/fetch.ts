export async function fetchData(nhsNumber?: number) {
  const apiUrl = `${process.env.API_URL}/api/participants?nhsnumber=${nhsNumber}`;

  const response = await fetch(apiUrl);
  if (!response.ok) {
    throw new Error(`Error fetching data: ${response.statusText}`);
  }
  return response.json();
}

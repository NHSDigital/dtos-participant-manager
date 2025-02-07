export async function fetchPatientData(nhsNumber: string) {
  try {
    const url = `${process.env.API_URL}/api/participants?nhsnumber=${nhsNumber}`;
    console.log(url);
    const response = await fetch(url);

    if (!response.ok) {
      throw new Error(`Error fetching data: ${response.statusText}`);
    }

    return await response.json();
  } catch (error) {
    console.error("Fetch error:", error);
    throw error;
  }
}

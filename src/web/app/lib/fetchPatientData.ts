export async function fetchPatientData(nhsNumber: string) {
  try {
    const url = `${process.env.API_URL}/api/participants?nhsnumber=${nhsNumber}`;
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

export async function fetchPatientScreeningEligibility(accessToken: string) {
  try {
    const url = `${process.env.EXPERIENCE_PORT}/api/GetScreeningEligibility`;
    const response = await fetch(url, {
      method: "GET",
      headers: {
        Authorization: "Bearer " + accessToken,
      },
    });

    if (!response.ok) {
      throw new Error(`Error fetching data: ${response.statusText}`);
    }

    return await response.json();
  } catch (error) {
    console.error("Fetch error:", error);
    throw error;
  }
}

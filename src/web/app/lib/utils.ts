export const formatDate = (dateString: string): string => {
  const date = new Date(dateString);
  const options: Intl.DateTimeFormatOptions = {
    year: "numeric",
    month: "long",
  };
  return date.toLocaleDateString("en-GB", options);
};

export function createUrlSlug(text: string): string {
  return text.toLowerCase().trim().split(/\s+/).join("-");
}

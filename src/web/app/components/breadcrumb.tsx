import Link from "next/link";

interface BreadcrumbItem {
  label: string;
  url: string;
}

interface BreadcrumbProps {
  readonly items: readonly BreadcrumbItem[];
}

export default function Breadcrumb({ items }: Readonly<BreadcrumbProps>) {
  const lastItem = items[items.length - 1];
  return (
    <nav className="nhsuk-breadcrumb" aria-label="Breadcrumb">
      <ol className="nhsuk-breadcrumb__list">
        {items.map((item) => (
          <li key={item.label} className="nhsuk-breadcrumb__item">
            <Link className="nhsuk-breadcrumb__link" href={item.url}>
              {item.label}
            </Link>
          </li>
        ))}
      </ol>
      {lastItem && (
        <p className="nhsuk-breadcrumb__back">
          <Link className="nhsuk-breadcrumb__link" href={lastItem.url}>
            <span className="nhsuk-u-visually-hidden">Back to </span>
            {lastItem.label}
          </Link>
        </p>
      )}
    </nav>
  );
}
